using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPToStart : MonoBehaviour
{
    private Vector3 startLoc;
    
    private void Start()
    {
        startLoc = transform.position;
    }

    public void TPBackToStart()
    {
        this.transform.position = startLoc;
    }
}
