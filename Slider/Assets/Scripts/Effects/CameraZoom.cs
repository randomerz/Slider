using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraZoom : Singleton<CameraZoom>
{
   // public CinemachineVirtualCamera cmCamera;

    public AnimationCurve MoveUpCurve;
    public AnimationCurve MoveDownCurve;


    public GameObject maincam;
    public GameObject movecam;
    public CinemachineVirtualCamera cam;
    public GameObject image;
    private void Awake() {
        InitializeSingleton();
    }

    public static void MoveUp(float duration)
    {
        _instance?.StartCoroutine(_instance.AdjustZoom(duration, _instance.MoveUpCurve));
    }

    public static void MoveDown(float duration)
    {
        _instance?.StartCoroutine(_instance.AdjustZoom(duration, _instance.MoveDownCurve));
    }


    public IEnumerator AdjustZoom(float duration, AnimationCurve curve)
    {
        //maincam.SetActive(false);
       // movecam.SetActive(true);
        image.transform.localScale = Vector3.one * 2;
        image.SetActive(true);
       // cam.m_Lens.OrthographicSize *= 2;
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
       // image.SetActive(false);
      //  cam.m_Lens.OrthographicSize /= 2;
        //movecam.SetActive(false);
        //maincam.SetActive(true);
    }
}
