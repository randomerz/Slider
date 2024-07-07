using UnityEngine;

public class ScrollTextEffects : MonoBehaviour
{
    public float duration;
    public float rotationStart;
    public float rotationEnd;
    public Vector3 offsetStart;
    public Vector3 offsetEnd;

    public AnimationCurve transformationCurve;
    public AnimationCurve radiusCurve;
    [Tooltip("Should be 0 at the start and 1 at the end.")]
    public AnimationCurve alphaCurve; 

    [Header("References")]
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;

    private Coroutine transformationCoroutine;

    public void DoEffect()
    {
        if (transformationCoroutine != null)
        {
            StopCoroutine(transformationCoroutine);
        }

        transformationCoroutine = CoroutineUtils.ExecuteEachFrame(
            (x) => {
                float trans = transformationCurve.Evaluate(x);
                float rads = radiusCurve.Evaluate(x);
                float alpha = alphaCurve.Evaluate(x);

                float radiusMultiplier = Mathf.Lerp(0, 1, rads);
                rectTransform.anchoredPosition = radiusMultiplier * Vector3.Lerp(offsetStart, offsetEnd, trans);
                rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(rotationStart, rotationEnd, trans));
                canvasGroup.alpha = 1 - x;
            },
            () => {
                rectTransform.anchoredPosition = Vector3.zero;
                rectTransform.rotation = Quaternion.identity;
                canvasGroup.alpha = 0;
            },
            this,
            duration
        );
    }
}