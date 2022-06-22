using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public Image blackBox;
    [SerializeField] private int i = 0;
    public string sceneToLoad;
    public List<GameObject> images;
    public List<GameObject> textboxes;
    public GameObject skipbox;
    public float dialoguePause;
    bool skipImages = false;//for skipping dialogue
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
            textboxes[x].GetComponent<TMPTextTyper>().SetTextSpeed(GameSettings.textSpeed * 1.5f);
        }
        images[0].SetActive(true);
        textboxes[0].SetActive(true);
        StartCoroutine(BoxFadeOut());
        StartCoroutine(scrolltext(textboxes[0]));


        listener = InputSystem.onAnyButtonPress.Call(ctrl => advanceCutscene());
        
        
        //StartCoroutine(cutscene());
    }

    void advanceCutscene()
    {
        skipImages = true;
    }

    public void exitCutscene()
    {
        listener.Dispose();
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
            StartCoroutine(BoxFadeOut());
            StartCoroutine(scrolltext(textboxes[i]));
            textboxes[i].SetActive(true);
            
        } else
        {
            exitCutscene();
        }
    }


    IEnumerator scrolltext(GameObject text)
    {
        skipImages = false;
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

    IEnumerator BoxFadeIn()
    {
        Color color = blackBox.color;
        while (color.a < 1f)
        {
            Debug.Log("Skip: " + skipImages);
            if (skipImages)
            {
                color.a += Time.deltaTime * 3f;
            }
            else
            {
                color.a += Time.deltaTime;
            }
            blackBox.color = color;
            yield return null;
        }
        skipImages = false;
    }

    IEnumerator BoxFadeOut()
    {
        Color color = blackBox.color;
        while (color.a > 0f)
        {
            Debug.Log("Skip: " + skipImages);
            if (skipImages)
            {
                color.a -= Time.deltaTime * 3f;
            }
            else
            {
                color.a -= Time.deltaTime;
            }
            blackBox.color = color;
            yield return null;
        }
        Debug.Log("Box faded out!");
        skipImages = false;
    }

    IEnumerator WaitToAdvance()
    {
        Debug.Log("Waiting!");
        skipImages = false;
        for (float time = 2f; time >= 0; time -= .2f)
        {
            if (!skipImages)
            {
                yield return new WaitForSeconds(.2f);
            }
        }
        skipImages = false;
        StartCoroutine(BoxFadeIn());
        yield return new WaitForSeconds(1.5f);
        advanceImages();
    }
}
