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
        }
        images[0].SetActive(true);
        textboxes[0].SetActive(true);
        StartCoroutine(BoxFadeOut());
        StartCoroutine(scrolltext(textboxes[0]));


        listener = InputSystem.onAnyButtonPress.Call(ctrl => advanceCutscene());
        
        
        //StartCoroutine(cutscene());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void advanceCutscene()
    {
        skipImages = true;
    }

    void stopAdvance()
    {

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
        TMP_Text tmp = text.GetComponent<TMP_Text>();

        string s1 = tmp.text;
        string s2 = "";
        tmp.text = "";
        //wait a bit so it doesnt start during fadein
        yield return new WaitForSeconds(0.2f);
        foreach (char letter in s1)
        {
            if (skipImages)
            {
                tmp.text = s1;
                break;
            }
            s2 += letter;
            tmp.text = (s2);

            yield return new WaitForSeconds(2 * GameSettings.textSpeed);
        }
        StartCoroutine(WaitToAdvance());
    }

    IEnumerator BoxFadeIn()
    {
        Color color = blackBox.color;
        while (color.a < 1f)
        {
            color.a += .2f;
            blackBox.color = color;
            yield return new WaitForSeconds(.1f);
        }
    }

    IEnumerator BoxFadeOut()
    {
        Color color = blackBox.color;
        while (color.a > 0f)
        {
            color.a -= .2f;
            blackBox.color = color;
            yield return new WaitForSeconds(.1f);
        }
        Debug.Log("Box faded out!");
    }

    IEnumerator WaitToAdvance()
    {
        StartCoroutine(BoxFadeOut());
        Debug.Log("Waiting!");
        skipImages = false;
        for (float time = 2f; time >= 0; time -= .2f)
        {
            if (!skipImages)
            {
                yield return new WaitForSeconds(.2f);
            }
        }
        StartCoroutine(BoxFadeIn());
        yield return new WaitForSeconds(2f);
        advanceImages();
    }
}
