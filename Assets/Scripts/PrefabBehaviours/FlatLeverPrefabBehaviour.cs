using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlatLeverPrefabBehaviour : PiecePrefabBehaviour
{

    private GameObject renderedPegObject;
    private GameObject renderedBottomMediumArmObject;

    protected override void pieceSpecificSetup(){
        pieceDisplayName = "Flat Lever";
        snapToLayer = 8;
        renderedPegObject = transform.Find("Peg v12:1").Find("Peg v12").Find("Body1 3").gameObject;
        renderedBottomMediumArmObject = transform.Find("Medium Arm").Find("Medium Arm v18:5").Find("Medium Arm v18 1").Find("Body1 2").gameObject;
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
        // toggle the large box collider used for raycasting (enabled iff kinematic)
        // Transform haloAndBoxColliderTrans = transform.Find("Peg v12:1").Find("Halo and Box Collider");
        // haloAndBoxColliderTrans.gameObject.GetComponent<BoxCollider>().enabled = kinematic;

        // // toggle the other colliders used for physics interactions (enabled iff not kinematic)
        // transform.Find("Long Arm (DO NOT MACHINE) v13:1").gameObject.GetComponent<BoxCollider>().enabled = !kinematic;
        // transform.Find("Peg v12:1").gameObject.GetComponent<CapsuleCollider>().enabled = !kinematic;
        // transform.Find("Medium Arm").Find("Collider").gameObject.GetComponent<BoxCollider>().enabled = !kinematic;

        // recursively toggle the isKinematic property on all descendents with a Rigidbody
        processParentKinematic(gameObject, kinematic);
    }

    // recursive helper function that toggles isKinematic for the given GameObject (parent), if it has a Rigidbody, and then calls itself on each child of that object
    private void processParentKinematic(GameObject parent, bool kinematic){
        Rigidbody rb = parent.GetComponent<Rigidbody>();
        if(rb != null){
            rb.isKinematic = kinematic;
        }
        foreach(Transform child_trans in parent.transform){
            processParentKinematic(child_trans.gameObject, kinematic);
        }
    }

    public override Behaviour getHalo(){
        return transform.Find("Peg v12:1").Find("Halo and Box Collider").gameObject.GetComponent("Halo") as Behaviour;
    }

    protected override float getHeight(){
        return getTop() - getBottom();
    }

    protected override float getTop(){
        return renderedPegObject.transform.position.y + renderedPegObject.GetComponent<MeshRenderer>().bounds.size.y;
    }

    protected override float getBottom(){
        return renderedBottomMediumArmObject.transform.position.y;
    }

    protected override float convertBottomToTransformY(float bottom){
        return bottom + transform.position.y - getBottom();
    }
}
