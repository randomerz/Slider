using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    private string[] dialogues;
    public TextMeshProUGUI dialogueText;

    // accessibility
    public GameObject canvas;
    public GameObject highContrastBG;

    public static bool doubleSizeMode = false;
    public static bool highContrastMode = false;

    // Use this for initialization
    void Start()
    {
        dialogues = new string[0];
        dialogueText.gameObject.SetActive(false);
    }

    void Update()
    {
        // sketch af
        if (Time.timeScale == 0 && canvas.activeSelf)
        {
            CheckContrast();
            CheckSize();
        }
    }

    public void DisplayDialogues(Dialogue dialogue, int state)
    {
        StartDialogue(dialogue);
        DisplaySentence(state);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogues = dialogue.dialogues;
    }

    public void DisplaySentence(int state)
    {
        //Debug.Log(state);

        CheckContrast();
        CheckSize();

        canvas.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        string sentence = dialogues[state];
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence (string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.03f);
        }
    }

    public void FadeAwayDialogue()
    {
        canvas.SetActive(false);
        dialogueText.gameObject.SetActive(false);
    }

    private void CheckContrast()
    {
        highContrastBG.SetActive(highContrastMode);
    }

    private void CheckSize()
    {
        if (doubleSizeMode)
        {
            canvas.transform.localScale = new Vector3(2, 2, 2);
        }
        else
        {
            canvas.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}