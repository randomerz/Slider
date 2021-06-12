using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Make DialogueManager gameobject
public class DialogueManager : MonoBehaviour
{
    //private variables
    private Queue<string> dialogues;

    //public variables
    //public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    //public Animator animator;

    // Use this for initialization
    void Start()
    {
        dialogues = new Queue<string>();
        //nameText.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);
    }

    // displays the sentence based by queuing up the dialogue with StartDialogue() and then displaying it with DisplayNextSentence()
    public void DisplayDialogues(Dialogue dialogue)
    {
        StartDialogue(dialogue);
        DisplaySentence();
    }

    // queues the next dialogues for DisplayNextSentence()
    public void StartDialogue(Dialogue dialogue)
    {
        //animator.SetBool("IsOpen", true);
        //nameText.text = dialogue.name;
        dialogues.Clear();
        foreach (string sentence in dialogue.dialogues)
        {
            dialogues.Enqueue(sentence);
        }
    }

    // displayes the sentences, uses TypeSentence enumerator to print sentence letter by letter
    public void DisplaySentence()
    {
        //nameText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        string sentence = dialogues.Dequeue();
        StartCoroutine(TypeSentence(sentence));
    }

    // prints letter by letter
    IEnumerator TypeSentence (string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    // choses which dialogue to pick depending on the scenario of the world
    void WhichDialogue()
    {
        
    }
}