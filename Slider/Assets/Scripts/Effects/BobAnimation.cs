using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobAnimation : MonoBehaviour
{
    private Vector3 origOffset;
    private float PPU = 16;

    [SerializeField] private float totalLength = 3f; 
    [SerializeField] private float firstDown = 0.5f; 
    [SerializeField] private float backMiddle = 1.5f;
    [SerializeField] private float firstUp = 2f;
    private int state; // this is my attempt to make it more effecient

    void Start()
    {
        origOffset = transform.localPosition;
    }

    void Update()
    {
        float t = Time.time % totalLength;

        switch (state) 
        {
            case 0:
                if (t >= firstDown)
                {
                    state = 1;
                    transform.localPosition = origOffset + (Vector3.down / PPU);
                }
                break;
            case 1:
                if (t >= backMiddle)
                {
                    state = 2;
                    transform.localPosition = origOffset;
                }
                break;
            case 2:
                if (t >= firstUp)
                {
                    state = 3;
                    transform.localPosition = origOffset + (Vector3.up / PPU);
                }
                break;
            case 3:
                if (t < firstDown) // cant be t > totalLength
                {
                    state = 0;
                    transform.localPosition = origOffset;
                }
                break;
        }
    }
}
