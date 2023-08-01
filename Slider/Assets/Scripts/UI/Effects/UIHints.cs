using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
public class UIHints : MonoBehaviour
{
    public static UIHints instance { get; private set; }

    // I would make this a queue but you can't queue.Remove
    // public List<string> hintTexts = new List<string>(); 
    // public List<string> hintIDs = new List<string>(); //C: fixes some potential issues w/ exact hint text
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
        instance = this;
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += Clear;
    }

    private void Update() {
        tmproText.text = activeHint != null ? activeHint.GetFormattedHintText() : "";
    }

//   //  private void OnDisable()
//     {
//         SceneManager.activeSceneChanged -= Clear;
//     }

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
    /// <param name="hint">String of the hint</param>
    /// <param name="hintID">String ID of the hint</param>
    public static void AddHint(HintData hintData) { instance._AddHint(hintData); }

    public void _AddHint(HintData hintData)
    {
        hintList.Add(hintData);
        UpdateHint();
    }

    /// <summary>
    /// Removes a hint from the list of hints
    /// </summary>
    /// <param name="hintID">ID of the hint that was added</param>
    public static void RemoveHint(string hintID = "") { instance._RemoveHint(hintID); }

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
            //C: Switched UpdateHint to be in callback
            StartCoroutine(FadeHintBox(1, 0, () => {
                    activeHint = null;
                }));
        }
        }
        
    }
   
    private void UpdateHint()
    {   
        if(!isVisible)
        {
            if (hintList.Count > 0)
            {
                // fade hint box in
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

    private IEnumerator WaitForRemoval()
    {
        while(isFading)
        {
            yield return null;
        }
        UpdateHint();
    }

    // private void Clear(Scene current, Scene next) 
    // {
    //     StopAllCoroutines();
    //     hintIDs.Clear();
    //     hintTexts.Clear();
    //     canvasGroup.alpha = 0;    
    //     isVisible = false;
    // }

    // /// <summary>
    // /// Adds a hint to the list of hints to be displayed, shown in order added
    // /// </summary>
    // /// <param name="hint">String of the hint</param>
    // /// <param name="hintID">String ID of the hint</param>
    // public static void AddHint(string hint, string id) { instance._AddHint(hint, id); }

    // public void _AddHint(string hint, string id)
    // {
    //     hintTexts.Add(hint);
    //     hintIDs.Add(id);
    //     UpdateHint();
    // }

    // /// <summary>
    // /// Removes a hint from the list of hints
    // /// </summary>
    // /// <param name="hintID">ID of the hint that was added</param>
    // public static void RemoveHint(string hintID = "") { instance._RemoveHint(hintID); }

    // public void _RemoveHint(string hintID)
    // {
    //     if(hintID.Equals("") && hintRemovalQueue.Count > 0)
    //     {
    //         hintID = hintRemovalQueue[0];
    //         hintRemovalQueue.Remove(hintID);
    //     }
    //     int index = hintIDs.IndexOf(hintID);
    //     if (index == -1) 
    //         return; //C: This happens often and is okay

    //     if(isFading)
    //     {
    //         hintRemovalQueue.Add(hintID);
    //         StartCoroutine(WaitForRemoval());
    //     }
    //     else
    //     {
    //         string hint = hintTexts[index];
    //         if (!hintTexts.Remove(hint)) {
    //             Debug.LogWarning("Tried and failed to remove hint: " + hint); //C: This should not happen
    //             return;
    //         }
    //         hintIDs.Remove(hintID);
    //         if(index == 0) {
    //         //C: Switched UpdateHint to be in callback
    //         StartCoroutine(FadeHintBox(1, 0, () => {
    //                 tmproText.text = "";
    //             }));
    //     }
    //     }
        
    // }
   
    // private void UpdateHint()
    // {   
    //     if(!isVisible)
    //     {
    //         if (hintTexts.Count > 0)
    //         {
    //             // fade hint box in
    //             isVisible = true;
    //             tmproText.text = hintTexts[0];
    //             StartCoroutine(FadeHintBox(0, 1));
    //         }
    //     }
    //     else
    //     {
    //         if(hintRemovalQueue.Count > 0 && !isFading)
    //         {
    //             RemoveHint();
    //         }
    //         if (hintTexts.Count > 0 && tmproText.text.Equals("") )
    //         {
    //             tmproText.text = hintTexts[0];
    //             StartCoroutine(FadeHintBox(0, 1));
    //         }
    //         if (hintTexts.Count == 0)
    //         {
    //             isVisible = false;
    //             StartCoroutine(FadeHintBox(1, 0, () => {
    //                 tmproText.text = "";
    //             }));
    //         }
    //     }
    // }


    // private IEnumerator FadeHintBox(float from, float to, System.Action callback= null)
    // {
    //     isFading = true;
    //     float t = Mathf.Lerp(from, to, canvasGroup.alpha);
    //     while (t < fadeDuration)
    //     {
    //         canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);

    //         yield return null;

    //         t += Time.deltaTime;
    //     }

    //     canvasGroup.alpha = to;

    //     isFading = false;
    //     callback?.Invoke();
    //     UpdateHint();
    // }

    // private IEnumerator WaitForRemoval()
    // {
    //     while(isFading)
    //     {
    //         yield return null;
    //     }
    //     UpdateHint();
    // }
}

[System.Serializable]
public class HintDisplay
{ 
    public string keyboardHintText;
    public string controllerHintText;
}
