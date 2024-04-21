using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class OverlayCameraImageScaler : MonoBehaviour
{
    private int currScreenWidth;
    private int currScreenHeight;
    public RenderTexture rt;

    private void Awake()
    {
        UpdateRT();
    }
   
    private void Update()
    {
        if(Screen.width != currScreenWidth || Screen.height != currScreenHeight)
        {
            UpdateRT();
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
}
