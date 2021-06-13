using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public Transform baseTransform;

    public static CameraShake _instance;
    
    private static float curIntensity;

    void Awake()
    {
        if (baseTransform == null)
        {
            Debug.LogWarning("Camera Shake is missing base!");
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

            curTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _instance.baseTransform.position;
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

            curTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _instance.baseTransform.position;
    }
}