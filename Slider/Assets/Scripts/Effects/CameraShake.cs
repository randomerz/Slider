using UnityEngine;
using System.Collections;
using Cinemachine;
using System;

public class CameraShake : MonoBehaviour
{
    public Transform baseTransform;
    public CinemachineVirtualCamera cmCamera;

    //public static CameraShake _instance;
    
    private static float curIntensity;
   // private static CinemachineBasicMultiChannelPerlin cmPerlin;
    private CinemachineBasicMultiChannelPerlin cmPerlin;

    public enum ShakeType
    {
        Decrease,
        Constant,
        Increase
    }

    public class CameraShakeArgs
    {
        public float duration;
        public float amount;
        public ShakeType shakeType;

        public CameraShakeArgs(float d, float a, ShakeType type)
        {
            duration = d;
            amount = a;
            shakeType = type;
        }
    }
    public static event EventHandler<CameraShakeArgs> OnCameraShake;
    public static event EventHandler<EventArgs> OnStopShake;

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

    private void OnEnable() {
        OnCameraShake += HandleCameraShake;
        OnStopShake += HandleStopShake;
    }

    private void OnDisable() {
        OnCameraShake -= HandleCameraShake;
        OnStopShake -= HandleStopShake;
    }

    private void HandleCameraShake(object sender, CameraShakeArgs e)
    {
        if (e.amount < curIntensity)
            return;
        StopAllCoroutines();
        StartCoroutine(cShake(e.duration, e.amount, e.shakeType));
    }

    private void HandleStopShake(object sender, EventArgs e)
    {
        StopAllCoroutines();
        transform.position = baseTransform.position;
        if(cmPerlin != null) cmPerlin.m_AmplitudeGain = 0;
    }
    

    public static void Shake(float duration, float amount)
    {
       CameraShakeArgs c = new CameraShakeArgs(duration, amount, ShakeType.Decrease);
       OnCameraShake?.Invoke(null, c);
    }

    public static void ShakeConstant(float duration, float amount)
    {
        CameraShakeArgs c = new CameraShakeArgs(duration, amount, ShakeType.Constant);
        OnCameraShake?.Invoke(null, c);
    }

    public static void ShakeIncrease(float duration, float amount)
    {
        CameraShakeArgs c = new CameraShakeArgs(duration, amount, ShakeType.Increase);
        OnCameraShake?.Invoke(null, c);
    }

    public IEnumerator cShake(float duration, float amount, ShakeType type)
    {
        amount *= SettingsManager.ScreenShake;

        float startAmount = amount;
        float endAmount = amount;

        switch(type)
        {
            case ShakeType.Decrease:
                endAmount = 0;
                break;
            case ShakeType.Increase:
                startAmount = 0;
                break;
            case ShakeType.Constant:
                break;
        }

        float curTime = 0;
        Vector3 origPos = transform.position;

        while (curTime <= duration)
        {
            if (Time.timeScale == 0)
                break;
            
            curIntensity = Mathf.Lerp(startAmount, endAmount, curTime / duration);
            transform.position = baseTransform.position + UnityEngine.Random.insideUnitSphere * curIntensity;
            if(cmPerlin != null) cmPerlin.m_AmplitudeGain = curIntensity;

            curTime += Time.deltaTime;

            yield return null;
        }

        transform.position = baseTransform.position;
        if(cmPerlin != null) cmPerlin.m_AmplitudeGain = 0;
    }


    /*
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
        amount *= SettingsManager.ScreenShake;

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
        amount *= SettingsManager.ScreenShake;

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
        amount *= SettingsManager.ScreenShake;

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

    public static void StopShake()
    {
        if (_instance == null)
            return;

        _instance.StopAllCoroutines();

        _instance.transform.position = _instance.baseTransform.position;
        if(cmPerlin != null) cmPerlin.m_AmplitudeGain = 0;
    }*/
}