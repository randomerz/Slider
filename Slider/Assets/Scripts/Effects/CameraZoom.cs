using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraZoom : Singleton<CameraZoom>
{
    public CinemachineVirtualCamera cmCamera;

    public AnimationCurve MoveUpCurve;
    public AnimationCurve MoveDownCurve;

    private int refX;
    private int refY; 
    private float refOrtho;
    

    private void Awake() {
        InitializeSingleton();
        refOrtho = cmCamera.m_Lens.OrthographicSize;
    }

    public static void MoveUp(float duration)
    {
        _instance.StartCoroutine(_instance.AdjustZoom(duration, _instance.MoveUpCurve));
    }

    public static void MoveDown(float duration)
    {
        _instance.StartCoroutine(_instance.AdjustZoom(duration, _instance.MoveDownCurve));
    }


    public IEnumerator AdjustZoom(float duration, AnimationCurve curve)
    {
        float curTime = 0;
        float zoomLevel = 1;
        while (curTime <= duration)
        {
            if (Time.timeScale == 0)
                break;
            
            zoomLevel = curve.Evaluate(curTime/duration);
            cmCamera.m_Lens.OrthographicSize = refOrtho * zoomLevel;
            curTime += Time.deltaTime;

            yield return null;
        }
        cmCamera.m_Lens.OrthographicSize = refOrtho;
    }
}
