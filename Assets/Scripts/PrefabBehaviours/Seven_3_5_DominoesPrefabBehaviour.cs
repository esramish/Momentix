using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Seven_3_5_DominoesPrefabBehaviour : PiecePrefabBehaviour
{

    GameObject oneDominoRenderedObject;

    protected override void pieceSpecificSetup(){
        pieceDisplayName = "Dominoes";
        snapToLayer = 16;
        oneDominoRenderedObject = transform.Find("One_Domino").Find("group_0_16777215").gameObject;
    }

    protected override void movePiece(Vector2 touchPosition){
        Vector2 screenTranslation = touchPosition - prevFrameTouchPosition;
        Vector3 currScreenPosition = mainCamera.WorldToScreenPoint(transform.position);
        Vector3 newScreenPosition = new Vector3(currScreenPosition.x + screenTranslation.x, currScreenPosition.y + screenTranslation.y, currScreenPosition.z);
        Vector3 newWorldPosition = mainCamera.ScreenToWorldPoint(newScreenPosition);
        if(!canMoveDown && newWorldPosition.y < transform.position.y){
            newWorldPosition.y = transform.position.y;
        }
        if(!canMoveTowardsNegX && newWorldPosition.x < transform.position.x){
            newWorldPosition.x = transform.position.x;
        }
        if(!canMoveTowardsPosX && newWorldPosition.x > transform.position.x){
            newWorldPosition.x = transform.position.x;
        }
        if(!canMoveTowardsNegZ && newWorldPosition.z < transform.position.z){
            newWorldPosition.z = transform.position.z;
        }
        if(!canMoveTowardsPosZ && newWorldPosition.z > transform.position.z){
            newWorldPosition.z = transform.position.z;
        }
        transform.position = newWorldPosition;
        prevFrameTouchPosition = touchPosition; // update for next time the finger moves
    }

    public override void setKinematic(bool kinematic){
        // This logic assumes the only rigidbodies are on the direct children of the root prefab gameobject
        foreach(Transform child_trans in transform){
            child_trans.gameObject.GetComponent<Rigidbody>().isKinematic = kinematic;
        }
    }

    public override void setTriggers(bool triggers){
        // This logic assumes that the only colliders we're interested in changing are on the direct children of the root prefab gameobject
        foreach(Transform child_trans in transform){
            child_trans.gameObject.GetComponent<Collider>().isTrigger = triggers;
        }
    }

    public override Behaviour getHalo(){
        return GetComponent("Halo") as Behaviour;
    }

    protected override float getHeight(){
        return oneDominoRenderedObject.GetComponent<MeshRenderer>().bounds.size.y;
    }

    protected override float getTop(){
        return transform.position.y + getHeight() / 2;
    }

    protected override float getBottom(){
        return transform.position.y - getHeight() / 2;
    }

    protected override float convertBottomToTransformY(float bottom){
        return bottom + getHeight() / 2;
    }
}
