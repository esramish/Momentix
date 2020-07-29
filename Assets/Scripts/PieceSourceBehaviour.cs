using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceSourceBehaviour : MonoBehaviour, IPointerDownHandler
{
    
    public GameObject piecePrefab;
    public GameObject mainScriptObject; // connected in editor
    private RaycastingBehaviour raycastingScript;
    
    // Start is called before the first frame update
    void Start()
    {
        raycastingScript = mainScriptObject.GetComponent<RaycastingBehaviour>();
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

        raycastingScript.SetTopButtonsVisible(true);

        // set resettable to false since the new piece doesn't have a place in the previous saved state
        raycastingScript.resetButtonScript.setResettable(false);

        GameObject newPiece = Instantiate(piecePrefab);
        newPiece.GetComponent<PiecePrefabBehaviour>().OnPieceTouchBegin();
    }

}
