using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class CameraShake : MonoBehaviour
{
    public Transform baseTransform;
    public CinemachineVirtualCamera cmCamera;

    public static CameraShake _instance;
    private static CinemachineBasicMultiChannelPerlin cmPerlin;
    public static List<CameraShakeData> shakeData = new List<CameraShakeData>();

    public class CameraShakeData
    {
        public float duration;
        public float startAmount;
        public float endAmount;
        public float elapsedTime;

        public CameraShakeData(float d, float s, float e)
        {
            duration = d;
            startAmount = s;
            endAmount = e;
        }

        public float GetIntensity()
        {
           return Mathf.Lerp(startAmount, endAmount, elapsedTime / duration);
        }
    }

    void Awake()
    {
        if (baseTransform == null)
        {
            Debug.LogWarning("Camera Shake is missing base!");
        }

        if (cmCamera != null)
        {
            cmPerlin = cmCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public static void Shake(float duration, float amount)
    {
        amount *= SettingsManager.ScreenShake;
       CameraShakeData data = new CameraShakeData(duration, amount, 0);
       shakeData.Add(data);
    }

    public static void ShakeConstant(float duration, float amount)
    {
        amount *= SettingsManager.ScreenShake;
        CameraShakeData data = new CameraShakeData(duration, amount, amount);
        shakeData.Add(data);
    }

    public static void ShakeIncrease(float duration, float amount)
    {
        amount *= SettingsManager.ScreenShake;
        CameraShakeData data = new CameraShakeData(duration, 0, amount);
        shakeData.Add(data);
    }

    public static void StopShake()
    {
        shakeData.Clear();
    }

    private void Update() {
        if(Time.timeScale == 0) return;
        Shake(FindMaxShake());
    }

    private float FindMaxShake()
    {
        float maxShake = 0;
        if(shakeData == null) return 0;
        for(int i = 0; i < shakeData.Count; i++)
        {
            CameraShakeData data = shakeData[i];
            if(data == null || data.elapsedTime > data.duration)
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
        if(cmPerlin != null) cmPerlin.m_AmplitudeGain = amount;
    }
}