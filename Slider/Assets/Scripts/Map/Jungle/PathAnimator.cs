using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathAnimator : MonoBehaviour
{
    public int creationSpeed = 1;
    public float blobspeed = 1f;

    private int count = 0;
    public void CreateBlob()
    {

    }

    void Update()
    {
        if (count >= creationSpeed * 1000000)
        {
            CreateBlob();
        }
    }

    public void UpdateBlobVisibility()
    {

    }

}
