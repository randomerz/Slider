using System;
using System.Collections;
using UnityEngine;

public class Bump : MonoBehaviour
{
    public bool useRectTransform = true;
    public Vector3 bumpDifference = new Vector3(0, 1f / 16f);
    public float bumpDuration = 0.25f;
    public AnimationCurve curve;

    private RectTransform rectTransform;
    private Vector3 basePosition;

    private void Awake() 
    {
        if (useRectTransform)
        {
            rectTransform = GetComponent<RectTransform>();
            basePosition = rectTransform.anchoredPosition;
        }
        else
        {
            basePosition = transform.position;
        }
    }

    public void DoBump(Action callback = null)
    {
        StopAllCoroutines();
        StartCoroutine(_DoBump(callback));
    }

    private IEnumerator _DoBump(Action callback=null)
    {
        float t = 0;

        while (t < bumpDuration)
        {
            float x = curve.Evaluate(t / bumpDuration);
            SetPosition(Vector3.Lerp(basePosition + bumpDifference, basePosition, x));

            yield return null;

            t += Time.deltaTime;
        }

        SetPosition(basePosition);

        callback?.Invoke();
    }

    private void SetPosition(Vector3 position)
    {
        if (useRectTransform)
        {
            rectTransform.anchoredPosition = position;
        }
        else
        {
            transform.position = position;
        }
    }
}