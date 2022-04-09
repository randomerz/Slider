using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public Animator blackBoxAnimator;
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
        blackBoxAnimator.SetBool("advance", true);
        skipImages = true;
    }

    void stopAdvance()
    {
        blackBoxAnimator.SetBool("advance", false);
    }

    public void exitCutscene()
    {
        listener.Dispose();
        SceneManager.LoadScene(sceneToLoad);
    }

    public void showImages()
    {
        if (i + 1 < images.Count)
        {
            images[i].SetActive(false);
            textboxes[i].SetActive(false);
            i++;
            images[i].SetActive(true);
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

        advanceCutscene();
    }
}
