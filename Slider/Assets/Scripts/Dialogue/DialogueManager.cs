using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//Make DialogueManager gameobject
public class DialogueManager : MonoBehaviour
{
    //private variables
    private Queue<string> dialogues;

    //public variables
    public Text nameText;
    public Text dialogueText;
    public Animator animator;

    // Use this for initialization
    void Start()
    {
        dialogues = new Queue<string>();
    }

    // displays the sentence based by queuing up the dialogue with StartDialogue() and then displaying it with DisplayNextSentence()
    public void DisplayDialogues(Dialogue dialogue)
    {
        StartDialogue(dialogue);
        DisplayNextSentence();

    }

    // queues the next dialogues for DisplayNextSentence()
    public void StartDialogue(Dialogue dialogue)
    {
        animator.SetBool("IsOpen", true);
        nameText.text = dialogue.name;
        dialogues.Clear();
        foreach (string sentence in dialogue.dialogues)
        {
            dialogues.Enqueue(sentence);
        }
    }

    // displayes the sentences, uses TypeSentence enumerator to print sentence letter by letter
    public void DisplayNextSentence()
    {
        if (dialogues.Count == 0)
        {
            EndDialogue();
            return;
        }

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

    // closes the dialogue
    void EndDialogue()
    {
        animator.SetBool("IsOpen", false);
    }
}