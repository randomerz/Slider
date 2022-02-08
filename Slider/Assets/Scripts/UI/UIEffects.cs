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

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        
    }

    public static void FadeFromBlack(System.Action callback=null)
    {
        _instance.StartCoroutine(_instance.FadeCoroutine(_instance.blackPanel, _instance.blackPanelCanvasGroup, 1, 0, callback));
    }

    public static void FadeToBlack(System.Action callback=null)
    {
        _instance.StartCoroutine(_instance.FadeCoroutine(_instance.blackPanel, _instance.blackPanelCanvasGroup, 0, 1, callback));
    }

    public static void FlashWhite()
    {
        Debug.Log("Flashing!");
        _instance.StartCoroutine(_instance.FlashCoroutine());
    }


    private IEnumerator FadeCoroutine(GameObject gameObject, CanvasGroup group, float startAlpha, float endAlpha, System.Action callback=null)
    {
        float t = 0;
        gameObject.SetActive(true);
        group.alpha = startAlpha;

        while (t < fadeDuration)
        {
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);

            yield return null;
            t += Time.deltaTime;
        }

        group.alpha = endAlpha;
        gameObject.SetActive(false);

        callback?.Invoke();
    }

    private IEnumerator FlashCoroutine()
    {
        float t = 0;
        whitePanel.SetActive(true);
        whitePanelCanvasGroup.alpha = 0;

        while (t < flashDuration / 2)
        {
            whitePanelCanvasGroup.alpha = Mathf.Lerp(0, 1, t / (flashDuration / 2));

            yield return null;
            t += Time.deltaTime;
        }

        while (t >= 0)
        {
            whitePanelCanvasGroup.alpha = Mathf.Lerp(0, 1, t / (flashDuration / 2));

            yield return null;
            t -= Time.deltaTime;
        }

        whitePanelCanvasGroup.alpha = 0;
        whitePanel.SetActive(false);
    }
}
