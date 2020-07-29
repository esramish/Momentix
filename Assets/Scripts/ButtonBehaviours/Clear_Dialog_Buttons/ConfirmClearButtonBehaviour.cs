using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmClearButtonBehaviour : MonoBehaviour
{
    public GameObject mainScriptObject; // connected in editor
    RaycastingBehaviour raycastingScript;
    public GameObject resetObject; // connected in editor
    ResetButtonBehaviour resetButtonScript; 
    public GameObject clearDialogPanel; // connected in editor
    
    // Start is called before the first frame update
    void Start()
    {
        raycastingScript = mainScriptObject.GetComponent<RaycastingBehaviour>();
        resetButtonScript = resetObject.GetComponent<ResetButtonBehaviour>();
    }

    public void OnConfirmClearButtonPress(){
        foreach (GameObject piece in raycastingScript.pieces){
            Destroy(piece);
        }
        foreach (GameObject piece in raycastingScript.piecesRemovedWhileResettable){
            Destroy(piece); // since they were merely set to be inactive, not actually destroyed, before
        }
        raycastingScript.pieces.Clear();
        raycastingScript.piecesRemovedWhileResettable.Clear();
        resetButtonScript.setResettable(false);

        // put this after the list is cleared so it knows to undisplay the piece controls panel, etc.
        // also put it after resettable is set to false so it knows to redisplay the scroll view, etc.
        raycastingScript.ClearActivePiece(); 

        // undisplay the dialog and resume raycasting detection
        raycastingScript.clearDialogShowing = false;
        clearDialogPanel.SetActive(false);
    }
}
