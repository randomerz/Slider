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
    private int i = 0;
    public string sceneToLoad;
    public List<GameObject> images;
    public List<GameObject> textboxes;
    public GameObject skipbox;
    public float dialoguePause;

    // Start is called before the first frame update
    void Start()
    {
        if (images.Count != textboxes.Count)
        {
            throw new System.Exception("Number of textboxes in cutscene must equal number of images ");
        }
        for (int x = 0; x < images.Count; x++)
        {
            images[x].SetActive(false);
            textboxes[x].SetActive(false);
        }

        InputSystem.onAnyButtonPress.CallOnce(ctrl => showskip());
        
        StartCoroutine(cutscene());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void showskip()
    {
        skipbox.SetActive(true);
        InputSystem.onAnyButtonPress.CallOnce(ctrl => exitCutscene());
    }

    private IEnumerator cutscene()
    {
        while (i < images.Count)
        {

            images[i].SetActive(true);
            textboxes[i].SetActive(true);
            StartCoroutine(scrolltext(textboxes[i]));
            blackBoxAnimator.SetBool("fadeout", true);

            //this is really scuffed
            //idk how to do this without hard coding it
            //sorry
            yield return new WaitForSeconds(dialoguePause + 1.2f);
            blackBoxAnimator.SetBool("fadeout", false);
            blackBoxAnimator.SetBool("fadein", true);

            yield return new WaitForSeconds(1.3f);
            //fadein makes the images fade out, fadeout makes the images fade in
            //perfect
            blackBoxAnimator.SetBool("fadein", false);

            images[i].SetActive(false);
            textboxes[i].SetActive(false);

            i++;
        }
        exitCutscene();
    }

    public void exitCutscene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    IEnumerator scrolltext(GameObject text)
    {
        TMP_Text tmp = text.GetComponent<TMP_Text>();

        string s1 = tmp.text;
        string s2 = "";
        //wait a bit so it doesnt start during fadein
        yield return new WaitForSeconds(0.2f);
        foreach (char letter in s1)
        {
            s2 += letter;
            tmp.text = (s2);

            yield return new WaitForSeconds(0.09f);
        }
    }
}
