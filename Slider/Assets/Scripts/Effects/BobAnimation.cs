using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobAnimation : MonoBehaviour
{
    private Vector3 origOffset;
    private float PPU = 16;

    public float totalLength = 3f; 
    public float firstDown = 0.5f; 
    public float backMiddle = 1.5f;
    public float firstUp = 2f;
    public float initOffset = 0;
    private int state; // this is my attempt to make it more effecient

    public bool useRectTransform;
    private RectTransform rectTransform;

    public bool doHorizontalInstead;
    private Vector3 up;

    [Tooltip("Uses the totalLength to determine bob rate")]
    public bool doTwoPixelBobRate;

    void Start()
    {
        origOffset = transform.localPosition;
        if (useRectTransform) 
        {
            rectTransform = GetComponent<RectTransform>();
            origOffset = rectTransform.anchoredPosition;
        }
        
        up = doHorizontalInstead ? Vector3.right : Vector3.up;
    }

    void Update()
    {
        float t = (Time.time + initOffset) % totalLength;

        if (doTwoPixelBobRate) 
        {
            TwoPixelBob(t);
            return;
        }

        switch (state) 
        {
            case 0:
                if (t >= firstDown)
                {
                    state = 1;
                    if (!useRectTransform)
                        transform.localPosition = origOffset + (-up / PPU);
                    else
                        rectTransform.anchoredPosition = origOffset + (-up);
                }
                break;
            case 1:
                if (t >= backMiddle)
                {
                    state = (firstUp != -1) ? 2 : 0;
                    ResetPosition();
                }
                break;
            case 2:
                if (t >= firstUp)
                {
                    state = 3;
                    if (!useRectTransform)
                        transform.localPosition = origOffset + (up / PPU);
                    else
                        rectTransform.anchoredPosition = origOffset + up;
                }
                break;
            case 3:
                if (t < firstDown) // cant be t > totalLength
                {
                    state = 0;
                    ResetPosition();
                }
                break;
        }
    }

    private void TwoPixelBob(float t)
    {
        if (t < totalLength / 2)
        {
            if (!useRectTransform)
                transform.localPosition = origOffset + (-up / PPU);
            else
                rectTransform.anchoredPosition = origOffset + (-up);
        }
        else
        {
            if (!useRectTransform)
                transform.localPosition = origOffset;
            else
                rectTransform.anchoredPosition = origOffset;
        }
    }

    public void ResetPosition()
    {
        if (!useRectTransform)
            transform.localPosition = origOffset;
        else
            rectTransform.anchoredPosition = origOffset;
    }
}
