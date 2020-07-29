using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RampPrefabBehaviour : PiecePrefabBehaviour
{
    private GameObject renderedRampObject;

    protected override void pieceSpecificSetup(){
        pieceDisplayName = "Ramp";
        snapToLayer = 11;
        renderedRampObject = transform.Find("group_0_12568524").gameObject;
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
        gameObject.GetComponent<Rigidbody>().isKinematic = kinematic;
    }

    public override Behaviour getHalo(){
        return gameObject.GetComponent("Halo") as Behaviour;
    }

    protected override float getHeight(){
        return renderedRampObject.GetComponent<MeshRenderer>().bounds.size.y;
    }
    
    protected override float getTop(){
        return getBottom() + getHeight();
    }

    protected override float getBottom(){
        return renderedRampObject.transform.position.y;
    }

    protected override float convertBottomToTransformY(float bottom){
        return bottom + transform.position.y - getBottom();
    }
}
