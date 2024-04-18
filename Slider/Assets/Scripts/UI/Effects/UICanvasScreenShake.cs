using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Mostly from CameraShake.cs
public class UICanvasScreenShake : MonoBehaviour
{
    private static RectTransform rectTransform;
    private static Vector3 basePosition;
    private static List<CameraShake.CameraShakeData> shakeData = new List<CameraShake.CameraShakeData>();


    void Awake()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        basePosition = rectTransform.position;
    }

    public static void Shake(float duration, float amount)
    {
        amount *= SettingsManager.Setting<float>(Settings.ScreenShake).CurrentValue;
        CameraShake.CameraShakeData data = new CameraShake.CameraShakeData(duration, amount, 0, 0);
        shakeData.Add(data);
    }

    public static void ShakeConstant(float duration, float amount)
    {
        amount *= SettingsManager.Setting<float>(Settings.ScreenShake).CurrentValue;
        CameraShake.CameraShakeData data = new CameraShake.CameraShakeData(duration, amount, amount, 0);
        shakeData.Add(data);
    }

    public static void ShakeIncrease(float duration, float amount)
    {
        amount *= SettingsManager.Setting<float>(Settings.ScreenShake).CurrentValue;
        CameraShake.CameraShakeData data = new CameraShake.CameraShakeData(duration, 0, amount, 0);
        shakeData.Add(data);
    }

    public static void StopShake()
    {
        shakeData.Clear();
    }


    private void Update()
    {
        if (Time.timeScale == 0) return;
        Shake(FindMaxShake());
    }

    private float FindMaxShake()
    {
        float maxShake = 0;
        if (shakeData == null) return 0;
        for (int i = 0; i < shakeData.Count; i++)
        {
            CameraShake.CameraShakeData data = shakeData[i];
            if (data == null || data.elapsedTime > data.duration)
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
        rectTransform.position = basePosition + UnityEngine.Random.insideUnitSphere * amount;
        //if (cmPerlin != null) cmPerlin.m_AmplitudeGain = amount;
    }
}
