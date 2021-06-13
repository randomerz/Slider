using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    private string[] dialogues;
    public TextMeshProUGUI dialogueText;

    // Use this for initialization
    void Start()
    {
        dialogues = new string[0];
        dialogueText.gameObject.SetActive(false);
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
        dialogueText.gameObject.SetActive(true);
        string sentence = dialogues[state];
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence (string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void FadeAwayDialogue()
    {
        dialogueText.gameObject.SetActive(false);
    }
}