using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraRotateLeftButtonBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    private int camRotationDirection; // 0 is not moving, 1 moves left, -1 moves right
    public const float DEGREES_PER_SECOND = 45;

    // Start is called before the first frame update
    void Start()
    {
        camRotationDirection = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.gameObject.transform.RotateAround(Vector3.zero, Vector3.up, camRotationDirection * DEGREES_PER_SECOND * Time.deltaTime);
    }

    public void OnPointerDown(PointerEventData data){
        camRotationDirection = 1;
    }
    
    public void OnPointerUp(PointerEventData data){
        camRotationDirection = 0;
    }
}
