using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class PiecePrefabBehaviour : MonoBehaviour
{

    private const float SECONDS_TO_INITIAL_POSITION = 1;
    private const float SECONDS_TO_PLACEMENT_CORRECT = 0.5f;
    private const float THRESHOLD_ABOVE_FLOOR = 0.5f;
    private const float SNAP_TO_PIECE_RADIUS = 2;
    private const int MAX_SNAP_TO_COLLIDERS = 10;
    private bool setupComplete; // set to true once various object references are initialized. Don't set to false again after that!
    protected bool isNew; // the user hasn't stopped dragging the piece since it was instantiated
    protected bool moving; // the user is currently dragging 
    private Vector3 initialPosition; // the location where the piece was on the most recent touch begin, if the piece is currently moving
    private bool returningToInitialPosition; // the piece is currently returning to initialPosition
    private Vector3 initialPositionReturnFrameTranslation; // the vector the piece moves in a single update call when returningToInitialPosition == true
    private float initialPositionReturnTotalFrames; // the number of times Update will be called in the time it takes the piece to return to initialPosition. Calculated at runtime directly based on SECONDS_TO_INITIAL_POSITION
    private int initialPositionReturnFramesCompleted; // the number of times Update has been called so far since the piece started returning to initialPosition, if it's currently doing so
    
    private Vector3 placementCorrectionTarget; // the target location for the piece's placement auto-correction
    private bool placementCorrecting; // the piece is currently having its placement auto-corrected
    private Vector3 placementCorrectionFrameTranslation; // the vector the piece moves in a single update call when placementCorrecting == true
    private float placementCorrectionTotalFrames; // the number of times Update will be called in the time it takes to auto-correct the piece's placement. Calculated at runtime directly based on SECONDS_TO_PLACEMENT_CORRECT
    private int placementCorrectionFramesCompleted; // the number of times Update has been called so far since the piece started having its placement corrected, if it's currently doing so
    
    public bool canMoveDown;
    public bool canMoveTowardsNegX;
    public bool canMoveTowardsPosX;
    public bool canMoveTowardsNegZ;
    public bool canMoveTowardsPosZ;

    protected Camera mainCamera;
    protected RaycastingBehaviour raycastingScript;
    private float floorY;
    protected List<GameObject> pieces;
    protected GameObject pieceControlsPanel;
    private GameObject pieceControlsLabel;
    protected string pieceDisplayName;
    protected int snapToLayer;
    protected Vector2 prevFrameTouchPosition;

    private Behaviour halo;

    private Dictionary<GameObject, SavedTransformInfo> savedTransforms;
    private Dictionary<GameObject, SavedVelocityInfo> savedVelocities;

    // caution: this method could be run twice, not necessarily just once, right after the piece is instantiated
    void Start()
    {
        // initialize instance variables
        isNew = true;
        returningToInitialPosition = false;
        placementCorrecting = false;
        canMoveDown = true;
        canMoveTowardsNegX = true;
        canMoveTowardsPosX = true;
        canMoveTowardsNegZ = true;
        canMoveTowardsPosZ = true;
        mainCamera = Camera.main;
        raycastingScript = GameObject.Find("Main Script Object").GetComponent<RaycastingBehaviour>();
        GameObject floor = GameObject.Find("Floor");
        floorY = floor.transform.position.y;
        pieces = raycastingScript.pieces;
        pieceControlsLabel = raycastingScript.pieceControlsLabel;
        pieceControlsPanel = raycastingScript.pieceControlsPanel;
        pieceSpecificSetup();
        halo = getHalo();
        savedTransforms = new Dictionary<GameObject, SavedTransformInfo>();
        savedVelocities = new Dictionary<GameObject, SavedVelocityInfo>();

        // turn off physics for this object for now
        setKinematic(true);

        // move piece to position of touch (keeping it the same depth/distance from the camera as it was upon instantiation)
        // as seen in the fact that this is in Start, it should only happen upon instantiation. Position changes thereafter depend on how much the finger moves
        Vector3 currScreenPosition = mainCamera.WorldToScreenPoint(transform.position);
        Touch touch = Input.GetTouch(0);
        transform.position = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, currScreenPosition.z));
        
        // leave the following line as the last line of the method
        setupComplete = true;
    }

    // method that runs every time the user activates this piece, either by touching it, or by touching its corresponding 2D source and thus instantiating it
    public void OnPieceTouchBegin(){
        
        // this is to make sure instance variables are initialized even for the first touch, while avoiding running Start for subsequent touches
        // it DOESN'T guarantee that Start won't be run twice at the time the piece is initialized, if Start isn't atomic
        if(!setupComplete){
            Start();
        }
        
        // set initialPosition so that the piece can later revert to it if placed in an invalid position
        initialPosition = transform.position;

        // set prevFrameTouchPosition so the piece is ready to move if the user moves their finger
        prevFrameTouchPosition = Input.GetTouch(0).position;

        moving = true;
        pieceControlsPanel.SetActive(true);
        pieceControlsLabel.GetComponent<Text>().text = "Edit " + pieceDisplayName;
        if(raycastingScript.activePiece != null && raycastingScript.activePiece != gameObject){
            // disable the previous active piece's halo
            Behaviour otherPieceHalo = raycastingScript.activePiece.GetComponent<PiecePrefabBehaviour>().getHalo() as Behaviour;
            otherPieceHalo.enabled = false;
        }
        halo.enabled = true;
        raycastingScript.activePiece = gameObject;
        raycastingScript.piecesScrollView.GetComponent<ScrollRect>().enabled = false;
        raycastingScript.SetAllButtonsInteractable(false);
    }

    void Update()
    {
        if(returningToInitialPosition){
            if(initialPositionReturnFramesCompleted < initialPositionReturnTotalFrames){
                transform.Translate(initialPositionReturnFrameTranslation, Space.World);
                initialPositionReturnFramesCompleted++;
            }else{
                if(isNew){
                    Destroy(gameObject);
                    raycastingScript.ClearActivePiece();
                }else{
                    transform.position = initialPosition;
                    returningToInitialPosition = false;
                }
                reactivateInteractables();
            }
        }else if(placementCorrecting){
            if(placementCorrectionFramesCompleted < placementCorrectionTotalFrames){
                transform.Translate(placementCorrectionFrameTranslation, Space.World);
                placementCorrectionFramesCompleted++;
            }else{
                placementCorrecting = false;
                transform.position = placementCorrectionTarget;
                initialPosition = placementCorrectionTarget;
                if(isNew){
                    isNew = false;
                    pieces.Add(gameObject);
                }
                reactivateInteractables();
            }
        }else if(moving && Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled){
                moving = false;
                bool validPlacement = calculatePlacementCorrectionTarget();
                if(validPlacement){ // the piece was placed in a valid location
                    placementCorrectionTotalFrames = SECONDS_TO_PLACEMENT_CORRECT / Time.deltaTime;
                    placementCorrectionFramesCompleted = 0;
                    placementCorrectionFrameTranslation = (placementCorrectionTarget - transform.position) / placementCorrectionTotalFrames;
                    placementCorrecting = true;
                }else{ // the piece was placed in an invalid location
                    initialPositionReturnTotalFrames = SECONDS_TO_INITIAL_POSITION / Time.deltaTime;
                    initialPositionReturnFramesCompleted = 0;
                    initialPositionReturnFrameTranslation = (initialPosition - transform.position) / initialPositionReturnTotalFrames;
                    returningToInitialPosition = true;
                }
            }else if(touch.phase == TouchPhase.Moved){
                movePiece(touch.position);
            }
        }
    }

    private void reactivateInteractables(){
        raycastingScript.piecesScrollView.GetComponent<ScrollRect>().enabled = true;
        raycastingScript.SetAllButtonsInteractable(true);
    }

    // This method calculates a Vector3 representing the nearest valid target position, if there is one nearby, for placement auto-correction.
    // It then assigns that value (if it exists) to the corresponding instance variable, placementCorrectionTarget.
    // It returns a boolean representing whether or not it successfully calculated a nearby valid target position
    private bool calculatePlacementCorrectionTarget(){
        
        // detect up to MAX_SNAP_TO_COLLIDERS nearby snap-to objects that are relevant to this specific piece
        Collider[] colliders = new Collider[MAX_SNAP_TO_COLLIDERS];
        int numHit = Physics.OverlapSphereNonAlloc(transform.position, SNAP_TO_PIECE_RADIUS, colliders, 1 << snapToLayer);
        
        if(numHit > 0){ 
            // there are relevant snap-to colliders nearby, so find the nearest detected one and use its position as the target
            
            // Helper function that returns the distance from this piece to the given collider
            Func<Collider, float> distFromThis = (collider) => Vector3.Distance(transform.position, collider.gameObject.transform.position);
            
            // Helper function that indicates which of two colliders is closer to this piece
            Func<Collider, Collider, bool> isFirstDistanceSmaller = (first, second) => distFromThis(first) < distFromThis(second);
            
            // Identify the detected collider that's closest to this piece
            Collider closestCollider = colliders.Aggregate((first, second) => second == null || isFirstDistanceSmaller(first, second) ? first : second);
            
            // Use its position as the target
            placementCorrectionTarget = closestCollider.gameObject.transform.position;
            return true;
        }else if(getBottom() - floorY < THRESHOLD_ABOVE_FLOOR){
            // the piece is close to the floor, so its target is the spot on the floor directly below it
            placementCorrectionTarget = new Vector3(transform.position.x, convertBottomToTransformY(floorY), transform.position.z);
            return true;
        }
        
        // it's not close enough to any valid placement correction targets, so return false to indicate invalid placement
        return false;
    }

    protected abstract void pieceSpecificSetup();

    protected abstract void movePiece(Vector2 touchPosition);

    public abstract void setKinematic(bool kinematic);

    public abstract Behaviour getHalo();

    protected abstract float getHeight();
    protected abstract float getTop();
    protected abstract float getBottom();
    
    // returns the value for transform.position.y that would cause the bottom of this piece to be at the given coordinate
    // doesn't actually modify anything, just returns a value
    protected abstract float convertBottomToTransformY(float bottom);

    public void saveTransforms(){
        savedTransforms.Clear(); // make sure we don't have any leftovers from the previous saved state -- happens generally if setResettable(false) is given outside of the ResetButtonBehaviour class
        saveTransformsHelper(transform);
    }

    // recursive helper method
    private void saveTransformsHelper(Transform transform){
        SavedTransformInfo savedInfo = new SavedTransformInfo(transform.position, transform.rotation);
        savedTransforms.Add(transform.gameObject, savedInfo);
        foreach(Transform child_trans in transform){
            saveTransformsHelper(child_trans);
        }
    }

    public void resetTransforms(){
        resetTransformsHelper(transform);
        savedTransforms.Clear();
    }

    // recursive helper method
    private void resetTransformsHelper(Transform transform){
        SavedTransformInfo savedInfo = savedTransforms[transform.gameObject];
        transform.position = savedInfo.GetPosition();
        transform.rotation = savedInfo.GetRotation();
        foreach(Transform child_trans in transform){
            resetTransformsHelper(child_trans);
        }
    }

    public void saveVelocities(){
        savedVelocities.Clear(); // make sure we don't have any leftovers from the previous saved state
        saveVelocitiesHelper(transform);
    }

    // recursive helper method
    private void saveVelocitiesHelper(Transform transform){
        Rigidbody rb = transform.gameObject.GetComponent<Rigidbody>();
        if(rb != null){
            SavedVelocityInfo savedInfo = new SavedVelocityInfo(rb.velocity, rb.angularVelocity);
            savedVelocities.Add(transform.gameObject, savedInfo);
        }
        foreach(Transform child_trans in transform){
            saveVelocitiesHelper(child_trans);
        }
    }

    public void resumeVelocities(){
        resumeVelocitiesHelper(transform);
        savedVelocities.Clear();
    }

    private void resumeVelocitiesHelper(Transform transform){
        Rigidbody rb = transform.gameObject.GetComponent<Rigidbody>();
        if(rb != null){
            SavedVelocityInfo savedInfo = savedVelocities[transform.gameObject];
            rb.velocity = savedInfo.GetVelocity();
            rb.angularVelocity = savedInfo.GetAngularVelocity();
        }
        foreach(Transform child_trans in transform){
            resumeVelocitiesHelper(child_trans);
        }
    }

    public void clearVelocities(){
        savedVelocities.Clear();
    }

    public bool isMoving(){
        return moving;
    }

    public bool isReturningToInitialPosition(){
        return returningToInitialPosition;
    }

    public bool isPlacementCorrecting(){
        return placementCorrecting;
    }

    // A class for storing the primitive-like aspects of transforms so pieces can be reset later to those positions and rotations
    class SavedTransformInfo{
        private Vector3 position;
        private Quaternion rotation;

        public SavedTransformInfo(Vector3 position, Quaternion rotation){
            this.position = position;
            this.rotation = rotation;
        }

        public Vector3 GetPosition(){
            return position;
        }

        public Quaternion GetRotation(){
            return rotation;
        }
    }

    // A class for storing the primitive-like velocities of rigidbodies so pieces can be reset later to those if physics is resumed
    class SavedVelocityInfo{
        private Vector3 velocity;
        private Vector3 angularVelocity;

        public SavedVelocityInfo(Vector3 velocity, Vector3 angularVelocity){
            this.velocity = velocity;
            this.angularVelocity = angularVelocity;
        }

        public Vector3 GetVelocity(){
            return velocity;
        }

        public Vector3 GetAngularVelocity(){
            return angularVelocity;
        }
    }

}
