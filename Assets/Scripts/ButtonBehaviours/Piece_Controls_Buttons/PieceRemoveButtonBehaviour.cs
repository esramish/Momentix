using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceRemoveButtonBehaviour : MonoBehaviour
{
    public GameObject mainScriptObject; // connected in editor
    RaycastingBehaviour raycastingScript;
    public GameObject resetButtonObject; // connected in editor
    ResetButtonBehaviour resetButtonScript;
    
    // Start is called before the first frame update
    void Start()
    {
        raycastingScript = mainScriptObject.GetComponent<RaycastingBehaviour>();
        resetButtonScript = resetButtonObject.GetComponent<ResetButtonBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void removeActivePiece(){
        GameObject activePiece = raycastingScript.activePiece;
        PiecePrefabBehaviour activePieceScript = activePiece.GetComponent<PiecePrefabBehaviour>();
        
        // mark the snap-to target GameObject that the piece is currrently occupying, if there is one, as unoccupied
        GameObject target = activePieceScript.currOccupiedSnapToTarget;
        if(target != null){
            target.GetComponent<SnapToTargetBehaviour>().occupied = false;
        }

        // remove activePiece's colliders from the collidersInContact lists of any other GameObjects in contact with activePiece
        foreach(Collider colliderInContact in activePieceScript.collidersInContact){ // for each collider in contact with activePiece
            Transform parent = colliderInContact.gameObject.transform.parent;
            if(parent == null || parent.gameObject.name != "Workspace Boundaries"){ // presumably, therefore, the collider in question belongs to a machine piece that's in contact with activePiece
                PiecePrefabBehaviour pieceInContactScript = getCompletePiece(colliderInContact.gameObject).GetComponent<PiecePrefabBehaviour>(); // get that collider's piece and its script
                int lastIndex = pieceInContactScript.collidersInContact.Count - 1;
                for(int i = lastIndex; i >= 0; i--){ // for each collider in contact with the piece in question
                    Collider colliderInContactWithOtherPiece = pieceInContactScript.collidersInContact[i];
                    if(isDescendent(colliderInContactWithOtherPiece.gameObject.transform, activePiece.transform)){ // if the collider belongs to activePiece
                        // remove the collider from the collidersInContact list of the piece in question
                        pieceInContactScript.collidersInContact.RemoveAt(i);
                    }
                }
            }
        }

        // finally, actually take care of removing the piece
        if(resetButtonScript.getResettable()){
            raycastingScript.piecesRemovedWhileResettable.Add(activePiece);
            activePiece.SetActive(false);
        }else{
            Destroy(activePiece);
        }
        raycastingScript.pieces.Remove(activePiece); // needs to be before the call to clearActivePiece
        raycastingScript.ClearActivePiece();
    }

    // this recursive function works up the hierarchy looking for an object with a PiecePrefabBehaviour (implemented because of compound colliders)
    private GameObject getCompletePiece(GameObject child){
        if(child.GetComponent<PiecePrefabBehaviour>() != null){ 
            return child;
        }
        if(child.transform.parent == null){
            return null;
        }
        return getCompletePiece(child.transform.parent.gameObject);
    }

    // recursive function that works down a hierarchy to see if one transform is a descendent (including equivalent) of another
    private bool isDescendent(Transform descendent, Transform ancestor){
        if(descendent==ancestor){
            return true;
        }
        foreach(Transform anc_child in ancestor){
            if(isDescendent(descendent, anc_child)){
                return true;
            }
        }
        return false;
    }
}
