using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEffects : MonoBehaviour
{
    public GameObject blackPanel;
    public CanvasGroup blackPanelCanvasGroup;
    public float fadeDuration = 1;

    public GameObject whitePanel;
    public CanvasGroup whitePanelCanvasGroup;
    public float flashDuration = 1;

    private static UIEffects _instance;

    // Holds the last flashing/black fade coroutine called so we can end it when starting a new one
    private static Coroutine previousCoroutine;

    private void Awake()
    {
        _instance = this;
    }

    public static void FadeFromBlack(System.Action callback=null, float speed=1)
    {
        StartEffectCoroutine(_instance.FadeCoroutine(_instance.blackPanel, _instance.blackPanelCanvasGroup, 1, 0, callback, speed));
    }

    public static void FadeToBlack(System.Action callback=null, float speed=1)
    {
        StartEffectCoroutine(_instance.FadeCoroutine(_instance.blackPanel, _instance.blackPanelCanvasGroup, 0, 1, callback, speed));
    }

    public static void FadeFromWhite(System.Action callback=null, float speed=1)
    {
        StartEffectCoroutine(_instance.FadeCoroutine(_instance.whitePanel, _instance.whitePanelCanvasGroup, 0.9f, 0, callback, speed));
    }

    public static void FadeToWhite(System.Action callback=null, float speed=1)
    {
        StartEffectCoroutine(_instance.FadeCoroutine(_instance.whitePanel, _instance.whitePanelCanvasGroup, 0, 1, callback, speed));
    }

    public static void FlashWhite(System.Action callbackMiddle=null, System.Action callbackEnd=null, float speed=1)
    {
        StartEffectCoroutine(_instance.FlashCoroutine(callbackMiddle, callbackEnd, speed));
    }

    public static void ClearScreen()
    {
        _instance.blackPanel.SetActive(false);
        _instance.blackPanelCanvasGroup.alpha = 0;
        _instance.whitePanel.SetActive(false);
        _instance.whitePanelCanvasGroup.alpha = 0;
    }

    private static void StartEffectCoroutine(IEnumerator coroutine)
    {
        if (previousCoroutine != null)
        {
            _instance.StopCoroutine(previousCoroutine);
        }
        previousCoroutine = _instance.StartCoroutine(coroutine);
    }


    private IEnumerator FadeCoroutine(GameObject gameObject, CanvasGroup group, float startAlpha, float endAlpha, System.Action callback=null, float speed=1)
    {
        float t = 0;
        gameObject.SetActive(true);
        group.alpha = startAlpha;

        while (t < fadeDuration)
        {
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);

            yield return null;
            t += (Time.deltaTime * speed);
        }

        group.alpha = endAlpha;
        gameObject.SetActive(false);

        callback?.Invoke();
    }

    private IEnumerator FlashCoroutine(System.Action callbackMiddle=null, System.Action callbackEnd=null, float speed=1)
    {
        float t = 0;
        whitePanel.SetActive(true);
        whitePanelCanvasGroup.alpha = 0;

        while (t < flashDuration / 2)
        {
            whitePanelCanvasGroup.alpha = Mathf.Lerp(0, 1, t / (flashDuration / 2));

            yield return null;
            t += (Time.deltaTime * speed);
        }

        callbackMiddle?.Invoke();

        while (t >= 0)
        {
            whitePanelCanvasGroup.alpha = Mathf.Lerp(0, 1, t / (flashDuration / 2));

            yield return null;
            t -= (Time.deltaTime * speed);
        }

        whitePanelCanvasGroup.alpha = 0;
        whitePanel.SetActive(false);
        
        callbackEnd?.Invoke();
    }
}
