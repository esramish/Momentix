using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraRotateRightButtonBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    private int camRotationDirection; // 0 is not moving, 1 moves left, -1 moves right
    public const float DEGREES_PER_SECOND = 45;

    public GameObject mainScriptObject; // connected in editor
    private RaycastingBehaviour raycastingScript;

    // Start is called before the first frame update
    void Start()
    {
        camRotationDirection = 0;
        raycastingScript = mainScriptObject.GetComponent<RaycastingBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.gameObject.transform.RotateAround(raycastingScript.camPointOfRotation, raycastingScript.camAxisOfRotation, camRotationDirection * DEGREES_PER_SECOND * Time.deltaTime);
    }

    public void OnPointerDown(PointerEventData data){
        if(raycastingScript.activePiece != null){
            PiecePrefabBehaviour activePieceBehaviour = raycastingScript.activePiece.GetComponent<PiecePrefabBehaviour>();
            if(activePieceBehaviour.isMoving() || activePieceBehaviour.isPlacementCorrecting()){
                return;
            }
        }
        camRotationDirection = -1;
    }
    
    public void OnPointerUp(PointerEventData data){
        camRotationDirection = 0;
    }

    public bool rotating(){
        return camRotationDirection != 0;
    }
}
