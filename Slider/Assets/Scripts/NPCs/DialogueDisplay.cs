using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueDisplay : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI dialogueBG;
    public GameObject ping;

    public GameObject canvas;
    public GameObject highContrastBG;

    public bool doubleSizeMode = false;
    public bool highContrastMode = false;

    void Start()
    {
        ping.transform.position = new Vector2(transform.position.x, transform.position.y + 1);
    }

    public void DisplaySentence(string message)
    {
        CheckContrast();
        CheckSize();
        ReadMessagePing();
        canvas.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(TypeSentence(message));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        dialogueBG.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            dialogueBG.text += letter;
            yield return new WaitForSeconds(0.03f);
        }
    }

    public void FadeAwayDialogue()
    {
        canvas.SetActive(false);
    }

    public void NewMessagePing()
    {
        ping.SetActive(true);
    }

    public void ReadMessagePing()
    {
        ping.SetActive(false);
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
