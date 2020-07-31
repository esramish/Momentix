using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceSourceBehaviour : MonoBehaviour, IPointerDownHandler
{
    
    public GameObject piecePrefab; // connected in editor
    public GameObject mainScriptObject; // connected in editor
    private RaycastingBehaviour raycastingScript;
    public GameObject camLeftButton; // connected in editor
    private CameraRotateLeftButtonBehaviour camLeftButtonScript;
    public GameObject camRightButton; // connected in editor
    private CameraRotateRightButtonBehaviour camRightButtonScript;
    public GameObject pieceLeftButton; // connected in editor
    private PieceRotateLeftButtonBehaviour pieceLeftButtonScript;
    public GameObject pieceRightButton; // connected in editor
    private PieceRotateRightButtonBehaviour pieceRightButtonScript;
    
    // Start is called before the first frame update
    void Start()
    {
        raycastingScript = mainScriptObject.GetComponent<RaycastingBehaviour>();
        camLeftButtonScript = camLeftButton.GetComponent<CameraRotateLeftButtonBehaviour>();
        camRightButtonScript = camRightButton.GetComponent<CameraRotateRightButtonBehaviour>();
        pieceLeftButtonScript = pieceLeftButton.GetComponent<PieceRotateLeftButtonBehaviour>();
        pieceRightButtonScript = pieceRightButton.GetComponent<PieceRotateRightButtonBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData data){
        if(raycastingScript.startStopButtonScript.physicsOn){
            return;
        }

        if(raycastingScript.activePiece != null){
            PiecePrefabBehaviour activePieceBehaviour = raycastingScript.activePiece.GetComponent<PiecePrefabBehaviour>();
            if(activePieceBehaviour.isMoving() || activePieceBehaviour.isPlacementCorrecting()){
                return;
            }
        }

        if(camLeftButtonScript.rotating() || camRightButtonScript.rotating() || pieceLeftButtonScript.rotating() || pieceRightButtonScript.rotating()){
            return;
        }

        raycastingScript.SetTopButtonsVisible(true);

        // set resettable to false since the new piece doesn't have a place in the previous saved state
        raycastingScript.resetButtonScript.setResettable(false);

        GameObject newPiece = Instantiate(piecePrefab);
        PiecePrefabBehaviour newPieceScript = newPiece.GetComponent<PiecePrefabBehaviour>();
        newPieceScript.prefab = piecePrefab;
        newPieceScript.OnPieceTouchBegin();
    }

}
