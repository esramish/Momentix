using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelClearButtonBehaviour : MonoBehaviour
{
    public GameObject mainScriptObject; // connected in editor
    RaycastingBehaviour raycastingScript;
    public GameObject clearDialogPanel; // connected in editor
    
    // Start is called before the first frame update
    void Start()
    {
        raycastingScript = mainScriptObject.GetComponent<RaycastingBehaviour>();
    }

    public void OnCancelClearButtonPress(){
        raycastingScript.clearDialogShowing = false;
        clearDialogPanel.SetActive(false);
    }
}
