using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFadeIn : MonoBehaviour
{
    public GameObject blackPanel;
    public CanvasGroup panelCanvasGroup;
    public float fadeDuration = 1;

    void Start()
    {
        AudioManager.PlayMusic("Connection");
        StartCoroutine(FadeIn());
    }
    
    private IEnumerator FadeIn()
    {
        float t = 0;
        blackPanel.SetActive(true);
        panelCanvasGroup.alpha = 1;

        while (t < fadeDuration)
        {
            panelCanvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);

            yield return null;
            t += Time.deltaTime;
        }

        panelCanvasGroup.alpha = 0;
        blackPanel.SetActive(false);
    }
}
