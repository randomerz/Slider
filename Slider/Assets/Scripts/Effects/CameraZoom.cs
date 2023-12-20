using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraZoom : Singleton<CameraZoom>
{
    public AnimationCurve MoveUpCurve;
    public AnimationCurve MoveDownCurve;
    public GameObject image;
    public float extraDurationMultiplier;

    private void Awake() {
        InitializeSingleton();
    }

    public static void MoveUp(float duration)
    {
        _instance?.StopAllCoroutines();
        _instance?.StartCoroutine(_instance.AdjustZoom(duration, _instance.MoveUpCurve));
    }

    public static void MoveDown(float duration)
    {
        _instance?.StopAllCoroutines();
        _instance?.StartCoroutine(_instance.AdjustZoom(duration, _instance.MoveDownCurve));
    }


    public IEnumerator AdjustZoom(float duration, AnimationCurve curve)
    {
        duration *= extraDurationMultiplier;
        image.transform.localScale = Vector3.one * 2;
        image.SetActive(true);
        float curTime = 0;
        float zoomLevel = 1;
        while (curTime <= duration)
        {
            if (Time.timeScale == 0)
                break;
            
            zoomLevel = curve.Evaluate(curTime/duration);
            image.transform.localScale = new Vector3 (zoomLevel * 2, zoomLevel * 2, 2);
            curTime += Time.deltaTime;

            yield return null;
        }
        image.transform.localScale = Vector3.one * 2;
    }
}
