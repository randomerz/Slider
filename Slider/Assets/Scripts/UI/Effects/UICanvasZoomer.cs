using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvasZoomer : MonoBehaviour
{
    private RectTransform rectTransform;

    [SerializeField] private AnimationCurve curveZoomIn;
    [SerializeField] private AnimationCurve curveZoomReleaseSmall;
    [SerializeField] private AnimationCurve curveZoomReleaseBig;

    void Start()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    public void DoZoomIn(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(DoZoom(duration, curveZoomIn));
    }

    public void DoZoomReleaseSmall(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(DoZoom(duration, curveZoomReleaseSmall));
    }

    public void DoZoomReleaseBig(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(DoZoom(duration, curveZoomReleaseBig));
    }

    private IEnumerator DoZoom(float duration, AnimationCurve curve)
    {
        float t = 0;
        float x;

        while (t < duration)
        {
            x = curve.Evaluate(t / duration);
            rectTransform.localScale = Vector3.one * x;

            yield return null;
            t += Time.deltaTime;
        }

        x = curve.Evaluate(1);
        rectTransform.localScale = Vector3.one * x;
    }
}
