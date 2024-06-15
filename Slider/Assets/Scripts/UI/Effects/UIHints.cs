using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class UIHints : Singleton<UIHints>
{
    public List<string> hintRemovalQueue = new List<string>();
    public List<HintData> hintList = new List<HintData>();

    private bool isVisible;
    public float fadeDuration;
    private bool isFading;

    // References
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI tmproText;

    private HintData activeHint;

    private void Awake()
    {
        InitializeSingleton(this);
    }

    // AT: entire UIEffect prefab is force respawned on every scene transition, UIHints will be destroyed every time
    //     and there is no need to clear
    private void OnEnable()
    {
        // SceneManager.activeSceneChanged += Clear;
    }

    private void Update() 
    {
        tmproText.text = activeHint != null ? activeHint.GetFormattedHintText() : "";
    }

    private void Clear(Scene current, Scene next) 
    {
        StopAllCoroutines();
        hintList.Clear();
        canvasGroup.alpha = 0;    
        isVisible = false;
    }

    /// <summary>
    /// Adds a hint to the list of hints to be displayed, shown in order added
    /// </summary>
    /// <param name="hintData">Hint data to be added</param>
    public static void AddHint(HintData hintData) { _instance._AddHint(hintData); }

    public void _AddHint(HintData hintData)
    {
        hintList.Add(hintData);
        UpdateHint();
    }

    /// <summary>
    /// Removes a hint from the list of hints
    /// </summary>
    /// <param name="hintID">ID of the hint to be removed</param>
    public static void RemoveHint(string hintID = "") { _instance._RemoveHint(hintID); }

    public void _RemoveHint(string hintID)
    {
        if(hintID.Equals("") && hintRemovalQueue.Count > 0)
        {
            hintID = hintRemovalQueue[0];
            hintRemovalQueue.Remove(hintID);
        }
        HintData hint = null;
        for(int i = 0; i < hintList.Count; i++)
        {
            if(hintList[i].hintName == hintID) hint = hintList[i];
        }
        if (hint == null) 
            return; //C: This happens often and is okay

        if(isFading)
        {
            hintRemovalQueue.Add(hintID);
            StartCoroutine(WaitForRemoval());
        }
        else
        {
            if (!hintList.Remove(hint)) {
                Debug.LogWarning("Tried and failed to remove hint: " + hint.hintName); //C: This should not happen
                return;
            }
            if(hintList.Count == 0) {
            StartCoroutine(FadeHintBox(1, 0, () => {
                    activeHint = null;
                }));
            }
            else {
                StartCoroutine(FadeToNextHint());
            }
        }
    }
   
    private void UpdateHint()
    {   
        if(!isVisible)
        {
            if (hintList.Count > 0)
            {
                isVisible = true;
                activeHint = hintList[0];
                StartCoroutine(FadeHintBox(0, 1));
            }
        }
        else
        {
            if(hintRemovalQueue.Count > 0 && !isFading)
            {
                RemoveHint();
            }
            if (hintList.Count > 0 && activeHint == null )
            {
                activeHint = hintList[0];
                StartCoroutine(FadeHintBox(0, 1));
            }
            if (hintList.Count == 0)
            {
                isVisible = false;
                StartCoroutine(FadeHintBox(1, 0, () => {
                    activeHint = null;
                }));
            }
        }
    }


    private IEnumerator FadeHintBox(float from, float to, System.Action callback= null)
    {
        isFading = true;
        float t = Mathf.Lerp(from, to, canvasGroup.alpha);
        while (t < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);

            yield return null;

            t += Time.deltaTime;
        }

        canvasGroup.alpha = to;

        isFading = false;
        callback?.Invoke();
        UpdateHint();
    }

    private IEnumerator FadeToNextHint()
    {
        isFading = true;
        float t = 0;
        while (t < fadeDuration)
        {
            tmproText.alpha = Mathf.Lerp(1, 0, t / fadeDuration);

            yield return null;

            t += Time.deltaTime;
        }
        
        activeHint = hintList[0];
        t = 0;

        while (t < fadeDuration)
        {
            tmproText.alpha = Mathf.Lerp(0, 1, t / fadeDuration);

            yield return null;

            t += Time.deltaTime;
        }
        isFading = false;
        UpdateHint();
    }

    private IEnumerator WaitForRemoval()
    {
        while(isFading)
        {
            yield return null;
        }
        UpdateHint();
    }
}