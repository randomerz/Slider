using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFadeIn : MonoBehaviour
{
    public GameObject blackPanel;
    public CanvasGroup blackPanelCanvasGroup;
    public float fadeDuration = 1;

    public GameObject whitePanel;
    public CanvasGroup whitePanelCanvasGroup;
    public float flashDuration = 1;

    private static UIFadeIn _instance;

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        AudioManager.PlayMusic("Connection");
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float t = 0;
        blackPanel.SetActive(true);
        blackPanelCanvasGroup.alpha = 1;

        while (t < fadeDuration)
        {
            blackPanelCanvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);

            yield return null;
            t += Time.deltaTime;
        }

        blackPanelCanvasGroup.alpha = 0;
        blackPanel.SetActive(false);
    }

    public static void FlashWhite()
    {
        _instance.StartCoroutine(_instance.Flash());
    }

    private IEnumerator Flash()
    {
        float t = 0;
        whitePanel.SetActive(true);
        whitePanelCanvasGroup.alpha = 1;

        while (t < flashDuration / 2)
        {
            whitePanelCanvasGroup.alpha = Mathf.Lerp(0, 1, t / (flashDuration / 2));

            yield return null;
            t += Time.deltaTime;
        }

        while (t >= 0)
        {
            whitePanelCanvasGroup.alpha = Mathf.Lerp(1, 0, t / (flashDuration / 2));

            yield return null;
            t -= Time.deltaTime;
        }

        whitePanelCanvasGroup.alpha = 0;
        whitePanel.SetActive(false);
    }
}
