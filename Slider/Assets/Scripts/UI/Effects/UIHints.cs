using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIHints : MonoBehaviour
{
    public static UIHints instance { get; private set; }

    // I would make this a queue but you can't queue.Remove
    private List<string> hintTexts = new List<string>(); 

    private bool isVisible;
    public float fadeDuration;

    // References
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI tmproText;

    private void Awake() 
    {
        instance = this;
    }

    /// <summary>
    /// Adds a hint to the list of hints to be displayed, shown in order added
    /// </summary>
    /// <param name="hint">String of the hint</param>
    public static void AddHint(string hint) { instance._AddHint(hint); }

    public void _AddHint(string hint)
    {
        hintTexts.Add(hint);
        UpdateHint();
    }

    /// <summary>
    /// Removes a hint from the list of hints
    /// </summary>
    /// <param name="hint">String of the hint that was added</param>
    public static void RemoveHint(string hint) { instance._RemoveHint(hint); }

    public void _RemoveHint(string hint)
    {
        if (!hintTexts.Remove(hint))
            Debug.LogWarning("Tried and failed to remove hint: " + hint);
        UpdateHint();
    }

    private void UpdateHint()
    {
        if (!isVisible)
        {
            if (hintTexts.Count > 0)
            {
                // fade hint box in
                tmproText.text = hintTexts[0];
                // StopAllCoroutines();
                StartCoroutine(FadeHintBox(0, 1));
                isVisible = true;
            }
            
        }
        else // isVisible
        {
            if (hintTexts.Count == 0)
            {
                // no more hints to display, fade out
                isVisible = false;
                StopAllCoroutines();
                StartCoroutine(FadeHintBox(1, 0, () => {
                    tmproText.text = "";
                }));
            }
            else if (tmproText.text != hintTexts[0])
            {
                // switch hints text, maybe we want to have text fade between later
                tmproText.text= hintTexts[0];
            }
        }
    }

    private IEnumerator FadeHintBox(float from, float to, System.Action callback=null)
    {
        float t = Mathf.Lerp(from, to, canvasGroup.alpha);
        Debug.Log("going from " + from + " to " + to + " start at " + t);

        while (t < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);

            yield return null;

            t += Time.deltaTime;
        }

        canvasGroup.alpha = to;

        callback?.Invoke();
    }
}
