using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetButtonBehaviour : MonoBehaviour
{
    // only true when physics has been turned on since the last reset (or since the launching of the Contraption Designer, if there hasn't been a reset yet)
    // keep this private so that other classes have to use the setter, which also toggles button's interactable property
    private bool resettable;
    public GameObject mainScriptObject; // connected in editor
    public GameObject piecesScrollView; // connected in editor
    public GameObject startStopObject; // connected in editor
    public GameObject pieceControlsPanel; // connected in editor
    private RaycastingBehaviour raycastingScript;
    private StartStopButtonBehaviour startStopButtonScript;
    
    // Start is called before the first frame update
    void Start()
    {
        resettable = false;
        raycastingScript = mainScriptObject.GetComponent<RaycastingBehaviour>();
        startStopButtonScript = startStopObject.GetComponent<StartStopButtonBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnResetButtonPress(){
        
        // reenable the start/stop button, in the event that it had been disabled temporarily through the temporary removal of the last piece
        startStopObject.GetComponent<Button>().interactable = true;

        // turn off physics, if it's on
        if(startStopButtonScript.physicsOn){
            startStopButtonScript.OnStartStopPress();
        }

        piecesScrollView.SetActive(true); // needs to be after the OnStartStopPress call    
        
        // Reactivate the temporarily removed pieces and move them to the normal pieces list
        foreach (GameObject piece in raycastingScript.piecesRemovedWhileResettable){
            piece.SetActive(true);
            raycastingScript.pieces.Add(piece);
        }
        raycastingScript.piecesRemovedWhileResettable.Clear();
        
        // Finally, actually do the resetting
        foreach (GameObject piece in raycastingScript.pieces){
            PiecePrefabBehaviour pieceScript = piece.GetComponent<PiecePrefabBehaviour>();
            pieceScript.resetTransforms();
            pieceScript.clearVelocities(); // needs to be after the OnStartStopPress call
        }

        setResettable(false); // needs to be after the OnStartStopPress call
    }

    // also toggles the button's interactable property, and reappears the piece controls panel if applicable
    public void setResettable(bool resettable){
        gameObject.GetComponent<Button>().interactable = resettable;
        this.resettable = resettable;
        if(!resettable){
            if(startStopButtonScript != null){
                startStopButtonScript.setButtonState("start");
            }
            pieceControlsPanel.SetActive(true);
        }
    }

    public bool getResettable(){
        return resettable;
    }
}
