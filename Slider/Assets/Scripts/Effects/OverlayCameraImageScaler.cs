using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class OverlayCameraImageScaler : MonoBehaviour
{
    private int currScreenWidth;
    private int currScreenHeight;
    public RenderTexture rt;
    public RectTransform borderTransform;

    private void Awake()
    {
        UpdateRT();
        UpdateBorderScale();
    }
   
    private void Update()
    {
        if(Screen.width != currScreenWidth || Screen.height != currScreenHeight)
        {
            UpdateRT();
            UpdateBorderScale();
        }
    }

    private void UpdateRT()
    {
        rt.Release();
        currScreenWidth = Screen.width;
        currScreenHeight = Screen.height;
        rt.width = currScreenWidth * 2;
        rt.height = currScreenHeight * 2;
        rt.Create();
    }

    private void UpdateBorderScale()
    {
        float ratio = (float)Screen.width / Screen.height;
        if(ratio >= 16/9.0f)
        {
            borderTransform.localScale = 3 * Vector3.one * Screen.height / 1080.0f;
        }
        else
        {
            borderTransform.localScale = 3 * Vector3.one * Screen.width / 1920.0f;
        }
    }
}
