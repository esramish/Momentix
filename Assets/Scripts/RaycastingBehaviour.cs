using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RaycastingBehaviour : MonoBehaviour
{
    private Camera mainCamera;
    private Vector2 prevFrameFirstTouchPosition;
    private Vector2 prevFrameSecondTouchPosition;

    enum TouchAction {
        None, 
        MovingPiece,
        RotatingCamera,
        ZoomingCamera, 
        PanningCamera
    }

    private TouchAction currTouchAction;
    
    public Vector3 camPointOfRotation;
    public Vector3 camAxisOfRotation;
    private float rotationDegreesPerPixel; // how many degrees the camera rotates when user's finger moves one pixel across the screen

    public List<GameObject> pieces; // used in other classes
    public List<GameObject> piecesRemovedWhileResettable; // used in other classes
    public GameObject piecesScrollView; // connected in editor
    public GameObject pieceControlsPanel; // connected in editor
    public GameObject pieceControlsLabel; // connected in editor

    public GameObject pieceRotateLeftButton; // connected in editor
    public GameObject pieceRotateRightButton; // connected in editor
    public GameObject pieceRemoveButton; // connected in editor
    public GameObject cameraRotateLeftButton; // connected in editor
    public GameObject cameraRotateRightButton; // connected in editor

    public GameObject activePiece; // used in other classes
    public GameObject startStopObject; // connected in editor
    public GameObject resetObject; // connected in editor
    public GameObject clearAllObject; // connected in editor
    private Button startStopButton;
    public Button resetButton; // used in StartStopButtonBehaviour
    private Button clearAllButton;
    public StartStopButtonBehaviour startStopButtonScript; // used in ResetButtonBehaviour and PieceSourceBehaviour, at least
    public ResetButtonBehaviour resetButtonScript; // used in StartStopButtonBehaviour and PieceSourceBehaviour, at least
    private EventSystem eventSystem;
    public GameObject canvas; // connected in editor
    private GraphicRaycaster graphicRaycaster;
    public bool clearDialogShowing;
    
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        currTouchAction = TouchAction.None;
        camPointOfRotation = Vector3.zero;
        camAxisOfRotation = Vector3.up;
        rotationDegreesPerPixel = 0.1F;
        pieces = new List<GameObject>();
        piecesRemovedWhileResettable = new List<GameObject>();
        pieceControlsPanel.SetActive(false);
        startStopButton = startStopObject.GetComponent<Button>();
        resetButton = resetObject.GetComponent<Button>();
        clearAllButton = clearAllObject.GetComponent<Button>();
        startStopButtonScript = startStopObject.GetComponent<StartStopButtonBehaviour>();
        resetButtonScript = resetObject.GetComponent<ResetButtonBehaviour>();
        eventSystem = GetComponent<EventSystem>(); // used here for graphic raycasting (i.e. knowing which 2D canvas item was touched)
        graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        clearDialogShowing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(clearDialogShowing){
            return; // don't want any raycasting detection while a clear all confirmation is showing
        }
        if(Input.touchCount == 1){ 
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began){ 
                handleFirstTouchBegin(touch);
            }else if(touch.phase == TouchPhase.Moved){
                handleOneTouchMove(touch);
            }else if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled){
                currTouchAction = TouchAction.None;
            }
        }else if(Input.touchCount == 2){
            Touch firstTouch = Input.GetTouch(0);
            Touch secondTouch = Input.GetTouch(1);
            if(secondTouch.phase == TouchPhase.Began){
                handleSecondTouchBegin(secondTouch);
            }else if(firstTouch.phase == TouchPhase.Moved || secondTouch.phase == TouchPhase.Moved){
                handleTwoTouchMove(firstTouch, secondTouch);
            }else if(firstTouch.phase == TouchPhase.Ended || firstTouch.phase == TouchPhase.Canceled){
                handleFirstTouchBegin(secondTouch);
            }else if(secondTouch.phase == TouchPhase.Ended || secondTouch.phase == TouchPhase.Canceled){
                handleFirstTouchBegin(firstTouch);
            }
        }
    }

    private void handleFirstTouchBegin(Touch touch){
        bool isPlacementCorrecting = activePiece != null && activePiece.GetComponent<PiecePrefabBehaviour>().isPlacementCorrecting();
        if(!resetButtonScript.getResettable() && !isPlacementCorrecting){ // don't want to detect piece touches while the machine is resettable or while placement correction is occurrring
            GameObject hitObject = getSignificant3DObjectHit(touch);
            if(hitObject != null){ // raycast hit a piece
                currTouchAction = TouchAction.MovingPiece;
                hitObject.GetComponent<PiecePrefabBehaviour>().OnPieceTouchBegin();
                return;
            }
        }

        // check if user touched any Canvas objects
        if(hitCanvasElement(touch)){
            return; // user touched something on the canvas, so don't bother with rotation/zooming/panning
        }

        // user didn't touch anything (e.g. buttons, scroll bar) on the canvas. 
        // Since we already know user didn't touch a significant 3D piece either, we'll assume they were either starting to rotate the camera or touching the screen to clear their piece selection. 
        // Either way, clear their piece selection and begin rotation
        ClearActivePiece();
        currTouchAction = TouchAction.RotatingCamera;
        prevFrameFirstTouchPosition = touch.position;
    }

    private void handleOneTouchMove(Touch touch){

        if(currTouchAction != TouchAction.RotatingCamera){
            return;
        }

        // only horizontal rotation for now
        mainCamera.gameObject.transform.RotateAround(camPointOfRotation, camAxisOfRotation, (touch.position - prevFrameFirstTouchPosition).x * rotationDegreesPerPixel);
        prevFrameFirstTouchPosition = touch.position;
    }

    private void handleSecondTouchBegin(Touch touch){
        return;
    }
    
    private void handleTwoTouchMove(Touch firstTouch, Touch secondTouch){
        return;
    }

    // casts a ray in the direction of touch and returns the machine piece that is hit, if one is hit
    private GameObject getSignificant3DObjectHit(Touch touch){
        Vector3 touchFarWorldCoords = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, mainCamera.farClipPlane));
        RaycastHit hit3D;
        bool hitSignificant3DObject = false; // true for machine pieces but false for the workspace, for example
        GameObject hitObject = null;
        // Debug.DrawRay(transform.position, touchFarWorldCoords, Color.white); // now that this script isn't attached to the camera, transform.position would need to be updated
        if(Physics.Raycast(mainCamera.gameObject.transform.position, touchFarWorldCoords - mainCamera.gameObject.transform.position, out hit3D)){
            hitObject = hit3D.collider.gameObject;
            hitSignificant3DObject = true;
            while(hitObject.GetComponent<PiecePrefabBehaviour>() == null){ // this loop works up the hierarchy looking for an object with a PiecePrefabBehaviour, for example in the case of compound colliders
                if(hitObject.transform.parent){
                    hitObject = hitObject.transform.parent.gameObject; // so now we'll check if the parent object has a PiecePrefabBehaviour
                }else{ // for example we had hit the workspace, which has a collider but no ancestors with a PiecePrefabBehaviour
                    hitSignificant3DObject = false;
                    break;
                }
            }
        }
        if(hitSignificant3DObject){
            return hitObject;
        }
        return null;
    }

    // cast a ray towards the canvas in the direction of touch and return a boolean representing whether or not a GameObject on the canvas was hit
    private bool hitCanvasElement(Touch touch){
        PointerEventData touchEventData = new PointerEventData(eventSystem);
        touchEventData.position = touch.position;
        List<RaycastResult> graphicsResults = new List<RaycastResult>(); // an out-parameter-like list, it seems
        graphicRaycaster.Raycast(touchEventData, graphicsResults);
        return graphicsResults.Count > 0;
    }

    // this method removes the active piece's halo (if there is an active piece), changes its colliders to triggers, sets the activePiece variable to null, and changes the state of canvas objects
    // this method does NOT remove a piece, but it does check if there are any pieces remaining (and deactivates the piece controls panel, etc. if there aren't)
    // it is fine to call this method in this or another class
    public void ClearActivePiece(){
        if(activePiece != null){
            Behaviour halo = activePiece.GetComponent<PiecePrefabBehaviour>().getHalo() as Behaviour;
            halo.enabled = false;
            activePiece.GetComponent<PiecePrefabBehaviour>().setTriggers(true);
            activePiece = null;
        }
        if(pieceControlsPanel.activeInHierarchy){
            pieceControlsLabel.GetComponent<Text>().text = "Touch a piece to edit it";
            SetAllPieceControlsButtonsInteractable(false);
        }
        if(pieces.Count == 0){
            pieceControlsPanel.SetActive(false);
            startStopButton.interactable = false;
            
            if(!resetButtonScript.getResettable()){ // don't make any of the following changes if the contraption is merely paused
                piecesScrollView.SetActive(true);
                startStopButtonScript.setButtonState("start");
                clearAllButton.interactable = false;
                SetTopButtonsVisible(false);
            }
        }
    }

    public void SetAllPieceControlsButtonsInteractable(bool active){
        if(!pieceControlsPanel.activeInHierarchy){
            return;
        }
        pieceRotateLeftButton.GetComponent<Button>().interactable = active;
        pieceRotateRightButton.GetComponent<Button>().interactable = active;
        pieceRemoveButton.GetComponent<Button>().interactable = active;
    }

    public void SetAllCameraButtonsInteractable(bool active){
        cameraRotateLeftButton.GetComponent<Button>().interactable = active;
        cameraRotateRightButton.GetComponent<Button>().interactable = active;
    }

    // Set all buttons as interactable or not. 
    // Note that the reset button will only have interactable set to true here if the pieces are currently resettable, 
    // and the piece controls buttons will only be set to interactable if there is an active piece (which there might not be, if the user somehow cleared the active piece before placement correction finished)
    public void SetAllButtonsInteractable(bool active){
        SetAllPieceControlsButtonsInteractable(active && activePiece != null);
        SetAllCameraButtonsInteractable(active);
        startStopButton.interactable = active;
        resetButton.interactable = active && resetButtonScript.getResettable();
        clearAllButton.interactable = active;
    }

    // Set the start/stop, reset, and clear all GameObjects as active or not
    public void SetTopButtonsVisible(bool visible){
        startStopObject.SetActive(visible);
        resetObject.SetActive(visible);
        clearAllObject.SetActive(visible);
    }
}
