using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISpotlightEffect : MonoBehaviour
{
    private const int REFERENCE_WIDTH = 320;
    private const int REFERENCE_HEIGHT = 180;

    public RectTransform spotlightRectTransform;
    public AnimationCurve animationCurve;
    private const float ANIMATION_DURATION = 1;

    private const float MAX_RADIUS = 640;
    private float targetRadius = 0;
    private Coroutine spotlightCoroutine;

    private bool endSpotlightEarlyFlag = false;

    public void DebugSpotlight()
    {
        StartSpotlight(new Vector2(100, 50), 48, duration: 1.5f);
    }


    // The UI position to set the center of the spotlight
    public void StartSpotlight(Vector2 positionPixel, float radiusPixel, float duration=2, System.Action onStart=null, System.Action onFinish=null)
    {
        if (spotlightCoroutine != null)
        {
            Debug.LogWarning($"Didn't start spotlight because one was already happening!");
            return;
        }
        spotlightRectTransform.anchoredPosition = positionPixel;
        targetRadius = radiusPixel;
        spotlightCoroutine = StartCoroutine(DoSpotlight(duration, onStart, onFinish));
    }

    public void EndSpotlightEarly()
    {
        endSpotlightEarlyFlag = true;
    }

    private IEnumerator DoSpotlight(float duration, System.Action onStart=null, System.Action onFinish=null)
    {
        spotlightRectTransform.gameObject.SetActive(true);
        endSpotlightEarlyFlag = false;

        onStart?.Invoke();

        yield return StartCoroutine(DoSpotlightOut(reversed: true));

        // Wait for duration seconds
        float t = 0;
        while (t < duration)
        {
            if (endSpotlightEarlyFlag)
                break;
            
            yield return null;
            t += Time.deltaTime;
        }

        yield return StartCoroutine(DoSpotlightOut(reversed: false));
        
        spotlightRectTransform.gameObject.SetActive(false);
        spotlightCoroutine = null;

        onFinish?.Invoke();
    }

    private IEnumerator DoSpotlightOut(bool reversed=false)
    {
        float t = 0;
        while (t < ANIMATION_DURATION)
        {
            float x = t / ANIMATION_DURATION;
            if (reversed)
            {
                x = 1 - x;
            }
            float r = Mathf.Lerp(targetRadius, MAX_RADIUS, animationCurve.Evaluate(x));
            spotlightRectTransform.sizeDelta = new Vector2(r, r);

            yield return null;
            t += Time.deltaTime;
        }

        float finalMult = animationCurve.Evaluate(reversed ? 0 : 1);
        float finalRadius = Mathf.Lerp(targetRadius, MAX_RADIUS, finalMult);
        spotlightRectTransform.sizeDelta = new Vector2(finalRadius, finalRadius);
    }
}
