using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceTriggerBehaviour : MonoBehaviour
{
    
    private RaycastingBehaviour raycastingScript;
    
    // Start is called before the first frame update
    void Start()
    {
        raycastingScript = GameObject.Find("Main Script Object").GetComponent<RaycastingBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other){
        Transform otherParent = other.gameObject.transform.parent;
        if(otherParent == null || otherParent.gameObject.name != "Workspace Boundaries"){ // presumably, therefore, it's a machine piece that had caused the collision
            GameObject collidingPiece = getCompletePiece(other.gameObject);
            GameObject thisPiece = getCompletePiece(gameObject);
            if(collidingPiece != thisPiece){ // make sure we don't add colliders that are actually part of this piece--this happens with flat levers, for example    
                PiecePrefabBehaviour collidingPieceScript = collidingPiece.GetComponent<PiecePrefabBehaviour>();
                collidingPieceScript.collidersInContact.Add(gameObject.GetComponent<Collider>());
            }
        }
    }

    private void OnTriggerExit(Collider other){
        Transform parent = other.gameObject.transform.parent;
        if(parent == null || parent.gameObject.name != "Workspace Boundaries"){ // presumably, therefore, it's a machine piece that had caused the collision
            PiecePrefabBehaviour collidingPieceScript = getCompletePiece(other.gameObject).GetComponent<PiecePrefabBehaviour>();
            collidingPieceScript.collidersInContact.Remove(gameObject.GetComponent<Collider>());
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
