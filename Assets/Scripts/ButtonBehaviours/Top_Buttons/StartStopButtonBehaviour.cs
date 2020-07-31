using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartStopButtonBehaviour : MonoBehaviour
{

    public GameObject mainScriptObject; // connected in editor
    public GameObject pieceControlsPanel; // connected in editor
    public GameObject piecesScrollView; // connected in editor
    public GameObject clearAllObject; // connected in editor
    public GameObject workspaceBoundariesObject; // connected in editor
    private RaycastingBehaviour raycastingScript;
    public bool physicsOn;

    // Start is called before the first frame update
    void Start()
    {
        raycastingScript = mainScriptObject.GetComponent<RaycastingBehaviour>();
        physicsOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStartStopPress(){
        physicsOn = !physicsOn;
        
        foreach (GameObject piece in raycastingScript.pieces){
            PiecePrefabBehaviour pieceScript = piece.GetComponent<PiecePrefabBehaviour>();
            if(physicsOn && !raycastingScript.resetButtonScript.getResettable()){
                // If physics is on, save positions, if positions haven't already been saved since the last reset (or since the Contraption Designer loaded, if there hasn't been a reset yet)
                // i.e. this code runs when the user pressed a button that said, "start"
                pieceScript.saveTransforms();
            }else if(physicsOn){
                // Here, physics is being turned on and has already been turned on before since the last reset, so resume those velocities
                // i.e. this code runs when the user pressed a button that said, "resume"
                pieceScript.resumeVelocities();
            }else{
                // Here, physics is being turned off, so save the velocities in case the user wants to resume subsequently
                // i.e. this code runs when the user pressed a button that said, "pause"
                pieceScript.saveVelocities();
            }
        }

        raycastingScript.resetButtonScript.setResettable(true);

        if(physicsOn){
            raycastingScript.ClearActivePiece(); // probably not necessary since pressing the button should clear active piece anyway, but might as well play it safe
            setButtonState("pause"); // change the text and color of the button
        }else{
            setButtonState("resume"); // change the text and color of the button
        }
        
        // disable piece control buttons (even when turning physics off, since we still don't have an active piece yet at that point. But it's all irrelevant anyway if we don't allow paused-machine editing)
        raycastingScript.SetAllPieceControlsButtonsInteractable(false);

        // toggle the visibility of elements that are only visible when physics is off
        clearAllObject.SetActive(!physicsOn);

        // turn off visibility of pieces scroll view and piece controls panel, since we don't want to allow user to add or edit pieces before resetting
        piecesScrollView.SetActive(false);
        pieceControlsPanel.SetActive(false);
        
        // toggle isKinematic and isTrigger for all pieces
        foreach(GameObject piece in raycastingScript.pieces){
            PiecePrefabBehaviour pieceScript = piece.GetComponent<PiecePrefabBehaviour>();
            pieceScript.setKinematic(!physicsOn);
            pieceScript.setTriggers(!physicsOn); // needs to be after the ClearActivePiece call
        }

        // toggle isTrigger for all workspace boundary colliders 
        foreach(Transform bound_trans in workspaceBoundariesObject.transform){
            GameObject boundary = bound_trans.gameObject;
            boundary.GetComponent<Collider>().isTrigger = !physicsOn;
        }
    }

    // set the visible state (i.e. the text and color) of the button. param state: can be "start" or "pause" or "resume"
    public void setButtonState(string state){
        Text button_text = GetComponentInChildren<Text>();
        switch(state){
            case "pause":
                button_text.text = "Pause";
                button_text.color = Color.black;
                break;
            case "start":
                button_text.text = "Start";
                button_text.color = new Color(0, 0.5f, 0, 1); // dark green
                break;
            case "resume":
                button_text.text = "Resume";
                button_text.color = new Color(0, 0.5f, 0, 1); // dark green
                break;
            default:
                break;
        }
    }
}
