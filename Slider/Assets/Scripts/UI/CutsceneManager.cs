using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;


public class CutsceneManager : MonoBehaviour
{
    public CanvasGroup canvas;
    [SerializeField] private int i = 0;
    public string sceneToLoad;
    public List<GameObject> images;
    public List<GameObject> textboxes;
    public GameObject skipbox;
    public float dialoguePause;
    bool skipImages = false; //for skipping dialogue
    bool speedFadeOut = false;
    private IDisposable listener;


    // Start is called before the first frame update
    void Start() 
    {
        if (images.Count != textboxes.Count)
        {
            throw new System.Exception("Number of textboxes in cutscene must equal number of images ");
        }
        for (int x = 1; x < images.Count; x++)
        {
            images[x].SetActive(false);
            textboxes[x].SetActive(false);
            textboxes[x].GetComponent<TMPTextTyper>().SetTextSpeed(GameSettings.textSpeed * 2f);
        }
        images[0].SetActive(true);
        textboxes[0].SetActive(true);
        StartCoroutine(FadeIn());
        StartCoroutine(scrolltext(textboxes[0]));

       // listener = InputSystem.onAnyButtonPress.Call(ctrl => advanceCutscene());
        listener = InputSystem.onEvent
                        .Where(e => e.HasButtonPress())
                        .Call(eventPtr =>
                        {
                            foreach (var button in InputControlExtensions.GetAllButtonPresses(eventPtr))
                            {
                                int pauseBindCount = Controls.Bindings.UI.Pause.bindings.Count;
                                bool advCut = true;
                                for(int i = 0; i < pauseBindCount; i++)
                                {
                                    if(MakeAlphaNumeric(Controls.Bindings.UI.Pause.bindings[0].path)
                                    .Equals(MakeAlphaNumeric(button.path)))
                                        advCut = false;
                                }
                                if(advCut)
                                    AdvanceCutscene();
                            }
                        });
        //StartCoroutine(cutscene());

        AudioManager.DampenMusic(this, 0.4f, 36000);
    }

    private string MakeAlphaNumeric(string input)
    {
        return Regex.Replace(input, "[^a-zA-Z0-9]", String.Empty);
    }
    
    void AdvanceCutscene()
    {
        skipImages = true;
    }

    public void exitCutscene()
    {
        Debug.Log("Exiting Cutscene");
        canvas.alpha = 0;
        listener.Dispose();

        AudioManager.StopDampen(this);

        SceneInitializer.profileToLoad = SaveSystem.Current;
        SceneManager.LoadScene(sceneToLoad);
    }

    public void advanceImages()
    {
        skipImages = false;
        if (i + 1 < images.Count)
        {
            images[i].SetActive(false);
            textboxes[i].SetActive(false);
            i++;
            images[i].SetActive(true);
            StartCoroutine(FadeIn());
            StartCoroutine(scrolltext(textboxes[i]));
            textboxes[i].SetActive(true);
            
        } else
        {
            exitCutscene();
        }
    }


    IEnumerator scrolltext(GameObject text)
    {
        TMPTextTyper tmp_typer = text.GetComponent<TMPTextTyper>();
        string s1 = textboxes[i].GetComponent<TextMeshProUGUI>().text;
        textboxes[i].GetComponent<TextMeshProUGUI>().text = "";
        //wait a bit so it doesnt start during fadein
        yield return new WaitForSeconds(0.2f);
        tmp_typer.StartTyping(s1);
        while (!tmp_typer.finishedTyping)
        {
            if (skipImages)
            {
                if (tmp_typer.TrySkipText()) break;
            }
            yield return null;
        }
        skipImages = false;
        StartCoroutine(WaitToAdvance());
    }

    IEnumerator FadeIn()
    {
        while (canvas.alpha < 1f)
        {
            if (skipImages)
            {
                break;
            }
            else
            {
                canvas.alpha += Time.deltaTime;
            }
            yield return null;
        }
        canvas.alpha = 1f;
    }

    IEnumerator FadeOut(System.Action callback = null)
    {
        while (canvas.alpha > 0f)
        {
            if (skipImages)
            {
                break;
            }
            else if (speedFadeOut)
            {
                canvas.alpha -= Time.deltaTime * 2;
            }
            else
            {
                canvas.alpha -= Time.deltaTime;
            }
            yield return null;
        }
        canvas.alpha = 0f;
        skipImages = false;
        speedFadeOut = false;
        callback?.Invoke();
    }

    IEnumerator WaitToAdvance()
    {
        skipImages = false;
        float time = 0;
        while (!skipImages && time < 3f)
        {
            time += Time.deltaTime;
            yield return null;
        }
        speedFadeOut = skipImages ? true : false;
        skipImages = false;
        StartCoroutine(FadeOut(() => advanceImages()));
    }
}
