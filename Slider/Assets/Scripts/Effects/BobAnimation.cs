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
    [SerializeField] private float initOffset = 0;
    private int state; // this is my attempt to make it more effecient

    public bool useRectTransform;
    private RectTransform rectTransform;

    public bool doHorizontalInstead;
    private Vector3 up;

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
                    state = 2;
                    if (!useRectTransform)
                        transform.localPosition = origOffset;
                    else
                        rectTransform.anchoredPosition = origOffset;
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
                    if (!useRectTransform)
                        transform.localPosition = origOffset;
                    else
                        rectTransform.anchoredPosition = origOffset;
                }
                break;
        }
    }
}
