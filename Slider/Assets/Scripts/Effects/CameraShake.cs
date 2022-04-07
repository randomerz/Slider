using UnityEngine;
using System.Collections;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public Transform baseTransform;
    public CinemachineVirtualCamera cmCamera;

    public static CameraShake _instance;
    
    private static float curIntensity;
    private static CinemachineBasicMultiChannelPerlin cmPerlin;

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

        _instance = this;
    }

    public static void Shake(float duration, float amount)
    {
        if (_instance == null || amount < curIntensity)
            return;
        _instance.StopAllCoroutines();
        _instance.StartCoroutine(_instance.cShake(duration, amount));
    }

    public IEnumerator cShake(float duration, float amount)
    {
        float curTime = 0;
        Vector3 origPos = transform.position;

        while (curTime <= duration)
        {
            if (Time.timeScale == 0)
                break;

            curIntensity = Mathf.Lerp(amount, 0, curTime / duration);
            transform.position = _instance.baseTransform.position + Random.insideUnitSphere * curIntensity;
            if(cmPerlin != null) cmPerlin.m_AmplitudeGain = curIntensity;

            curTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _instance.baseTransform.position;
        if(cmPerlin != null) cmPerlin.m_AmplitudeGain = 0;
    }

    public static void ShakeConstant(float duration, float amount)
    {
        if (_instance == null || amount < curIntensity)
            return;
        _instance.StopAllCoroutines();
        _instance.StartCoroutine(_instance.cConstantShake(duration, amount));
    }

    public IEnumerator cConstantShake(float duration, float amount)
    {
        float curTime = 0;
        Vector3 origPos = transform.position;
        curIntensity = amount;

        while (curTime <= duration)
        {
            if (Time.timeScale == 0)
                break;
            
            transform.position = _instance.baseTransform.position + Random.insideUnitSphere * curIntensity;
            if(cmPerlin != null) cmPerlin.m_AmplitudeGain = curIntensity;

            curTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _instance.baseTransform.position;
        if(cmPerlin != null) cmPerlin.m_AmplitudeGain = 0;
    }

    public static void ShakeIncrease(float duration, float amount)
    {
        if (_instance == null || amount < curIntensity)
            return;
        _instance.StopAllCoroutines();
        _instance.StartCoroutine(_instance.cIncreaseShake(duration, amount));
    }

    public IEnumerator cIncreaseShake(float duration, float amount)
    {
        float curTime = 0;
        Vector3 origPos = transform.position;
        curIntensity = amount;

        while (curTime <= duration)
        {
            if (Time.timeScale == 0)
                break;

            curIntensity = Mathf.Lerp(0, amount, curTime / duration);
            transform.position = _instance.baseTransform.position + Random.insideUnitSphere * curIntensity;
            if(cmPerlin != null) cmPerlin.m_AmplitudeGain = curIntensity;

            curTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _instance.baseTransform.position;
        if(cmPerlin != null) cmPerlin.m_AmplitudeGain = 0;
    }
}