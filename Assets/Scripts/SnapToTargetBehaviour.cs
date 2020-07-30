using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToTargetBehaviour : MonoBehaviour
{
    
    public bool occupied; // other classes will toggle this
    
    // Start is called before the first frame update
    void Start()
    {
        occupied = false;
    }

}
