using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIHints : MonoBehaviour
{
    public static UIHints instance { get; private set; }

    // I would make this a queue but you can't queue.Remove
    public List<string> hintTexts = new List<string>(); 
    public List<string> hintIDs = new List<string>(); //C: fixes some potential issues w/ exact hint text

    private bool isVisible;
    public float fadeDuration;

    // References
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI tmproText;

    private void Awake() 
    {
        instance = this;
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += Clear;
    }

    private void Clear(Scene current, Scene next) 
    {
        hintIDs.Clear();
        hintTexts.Clear();
        canvasGroup.alpha = 0;    
        isVisible = false;
    }

    /// <summary>
    /// Adds a hint to the list of hints to be displayed, shown in order added
    /// </summary>
    /// <param name="hint">String of the hint</param>
    /// <param name="hintID">String ID of the hint</param>
    public static void AddHint(string hint, string id) { instance._AddHint(hint, id); }

    public void _AddHint(string hint, string id)
    {
        hintTexts.Add(hint);
        hintIDs.Add(id);
        UpdateHint();
    }

    /// <summary>
    /// Removes a hint from the list of hints
    /// </summary>
    /// <param name="hintID">ID of the hint that was added</param>
    public static void RemoveHint(string hintID) { instance._RemoveHint(hintID); }

    public void _RemoveHint(string hintID)
    {
        int index = hintIDs.IndexOf(hintID);
        if (index == -1) 
            return; //C: This happens often and is okay
        string hint = hintTexts[index];
        if (!hintTexts.Remove(hint)) {
            Debug.LogWarning("Tried and failed to remove hint: " + hint); //C: This should not happen
            return;
        }
        hintIDs.Remove(hintID);
        hintTexts.Remove(hint);
        if(index == 0) {
            //C: Switched UpdateHint to be in callback
            StartCoroutine(FadeHintBox(1, 0, () => {
                    tmproText.text = ""; UpdateHint();
                }));
        }
    }

   

    private void UpdateHint()
    {   
        if(!isVisible)
        {
            if (hintTexts.Count > 0)
            {
                // fade hint box in
                isVisible = true;
                tmproText.text = hintTexts[0];
                StartCoroutine(FadeHintBox(0, 1));
            }
        }
        else
        {
            if (hintTexts.Count > 0 && tmproText.text.Equals("") )
            {
                tmproText.text = hintTexts[0];
                StartCoroutine(FadeHintBox(0, 1));
            }
            if (hintTexts.Count == 0)
            {
                isVisible = false;
            }
        }
    }

    private IEnumerator FadeHintBox(float from, float to, System.Action callback=null)
    {
        float t = Mathf.Lerp(from, to, canvasGroup.alpha);
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
