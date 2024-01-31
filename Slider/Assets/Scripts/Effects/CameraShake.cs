using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class CameraShake : MonoBehaviour
{
    public Transform baseTransform;
    [Tooltip("The camera attached to the player prefab.")]
    public CinemachineVirtualCamera cmCamera;
    [Tooltip("If there are other cameras in the scene that should also be shook, like dollies.")]
    public List<CinemachineVirtualCamera> otherCMCameras;
    
    private static List<CinemachineBasicMultiChannelPerlin> cmPerlins = new List<CinemachineBasicMultiChannelPerlin>();
    public static List<CameraShakeData> shakeData = new List<CameraShakeData>();

    public class CameraShakeData
    {
        public float duration;
        public float startAmount;
        public float endAmount;
        public float elapsedTime;
        public float delay;

        public CameraShakeData(float d, float s, float e, float d2)
        {
            duration = d;
            startAmount = s;
            endAmount = e;
            delay = d2;
        }

        public float GetIntensity()
        {
            if(elapsedTime < delay) return 0;
            return Mathf.Lerp(startAmount, endAmount, (elapsedTime - delay) / duration);
        }
    }

    void Awake()
    {
        if (baseTransform == null)
        {
            Debug.LogWarning("Camera Shake is missing base!");
        }

        cmPerlins.Clear();
        if (cmCamera != null)
        {
            cmPerlins.Add(cmCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>());
        }
        foreach (CinemachineVirtualCamera c in otherCMCameras)
        {
            cmPerlins.Add(c.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>());
        }
    }

    private void Start() 
    {
        CheckMissingCamerasWarning();
    }

    public static void Shake(float duration, float amount)
    {
        amount *= SettingsManager.ScreenShake;
        CameraShakeData data = new CameraShakeData(duration, amount, 0, 0);
        shakeData.Add(data);
    }

    public static void ShakeConstant(float duration, float amount)
    {
        amount *= SettingsManager.ScreenShake;
        CameraShakeData data = new CameraShakeData(duration, amount, amount, 0);
        shakeData.Add(data);
    }

    public static void ShakeIncrease(float duration, float amount)
    {
        amount *= SettingsManager.ScreenShake;
        CameraShakeData data = new CameraShakeData(duration, 0, amount, 0);
        shakeData.Add(data);
    }

    //Time of segement, value
    public static void ShakeCustom(List<Vector2> points)
    {
        for(int i = 1; i < points.Count; i++)
        {
            float prevT = points[i - 1].x;
            float start= points[i - 1].y;
            float t= points[i].x;
            float end = points[i].y;

            start *= SettingsManager.ScreenShake;
            end *= SettingsManager.ScreenShake;
            // print((t - prevT + 0.05f, start, end, prevT));
            CameraShakeData data = new CameraShakeData(t - prevT + 0.05f, start, end, prevT);
            shakeData.Add(data);
        }
    }

   

    public static void StopShake()
    {
        shakeData.Clear();
    }

    private void Update() {
        if (Time.timeScale == 0) return;
        Shake(FindMaxShake());
    }

    private float FindMaxShake()
    {
        float maxShake = 0;
        if(shakeData == null) return 0;
        for(int i = 0; i < shakeData.Count; i++)
        {
            CameraShakeData data = shakeData[i];
            if(data == null || data.elapsedTime > (data.duration + data.delay))
            {
                shakeData.RemoveAt(i);
                i--;
            }
            else
            {
                maxShake = Mathf.Max(data.GetIntensity(), maxShake);
                data.elapsedTime += Time.deltaTime;
            }
        }
        return maxShake;
    }

    private void Shake(float amount)
    {
        transform.position = baseTransform.position + UnityEngine.Random.insideUnitSphere * amount;
        foreach (CinemachineBasicMultiChannelPerlin perlin in cmPerlins) 
        {
            perlin.m_AmplitudeGain = amount;
        }
    }

    private void CheckMissingCamerasWarning()
    {
        foreach (CinemachineVirtualCamera vcam in FindObjectsOfType<CinemachineVirtualCamera>(includeInactive: true))
        {
            if (cmCamera != vcam && !otherCMCameras.Contains(vcam))
            {
                Debug.LogWarning($"[Cameras] Virtual Camera '{vcam.gameObject}' will not be affected by screen shake. Add it to 'otherCMCameras' if it should.");
            }
        }
    }
}