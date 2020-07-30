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
        // mark the piece's currently occupied snap-to target GameObject, if there is one, as unoccupied
        GameObject target = raycastingScript.activePiece.GetComponent<PiecePrefabBehaviour>().currOccupiedSnapToTarget;
        if(target != null){
            target.GetComponent<SnapToTargetBehaviour>().occupied = false;
        }

        if(resetButtonScript.getResettable()){
            raycastingScript.piecesRemovedWhileResettable.Add(raycastingScript.activePiece);
            raycastingScript.activePiece.SetActive(false);
        }else{
            Destroy(raycastingScript.activePiece);
        }
        raycastingScript.pieces.Remove(raycastingScript.activePiece); // needs to be before the call to clearActivePiece
        raycastingScript.ClearActivePiece();
    }
}
