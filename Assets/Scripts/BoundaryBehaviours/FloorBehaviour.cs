using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorBehaviour : MonoBehaviour
{

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

    private void OnTriggerEnter(Collider other){
        Transform parent = other.gameObject.transform.parent;
        if(parent == null || parent.gameObject.name != "Workspace Boundaries"){ // presumably, therefore, it's a machine piece causing the collision
            getCompletePiece(other.gameObject).GetComponent<PiecePrefabBehaviour>().canMoveDown = false;
        }
    }

    private void OnTriggerExit(Collider other){
        Transform parent = other.gameObject.transform.parent;
        if(parent == null || parent.gameObject.name != "Workspace Boundaries"){ // presumably, therefore, it's a machine piece that had caused the collision
            getCompletePiece(other.gameObject).GetComponent<PiecePrefabBehaviour>().canMoveDown = true;
        }
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
}
