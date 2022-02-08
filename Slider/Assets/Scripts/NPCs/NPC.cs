using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public string characterName;
    public List<DialogueConditionals> dconds;

    [SerializeField] private DialogueDisplay dialogueDisplay;

    private DialogueConditionals currMessage;

    // might need optimizing
    void Update()
    {
        foreach (DialogueConditionals d in dconds)
        {
            d.CheckConditions();
        }
        DialogueConditionals newDialogue = CurrentDialogue();
        if (currMessage != newDialogue)
        {
            currMessage = newDialogue;
            dialogueDisplay.NewMessagePing();
        }
    }

    public DialogueConditionals CurrentDialogue()
    {
        DialogueConditionals curr = null;
        int max = 0;
        for (int i = 0; i< dconds.Count; i++)
        {
            if (dconds[i].GetPriority() > max)
            {
                curr = dconds[i];
            }
        }
        return curr;
    }
    public void TriggerDialogue()
    {
        currMessage.OnDialogue();
        dialogueDisplay.DisplaySentence(currMessage.GetDialogue());
    }

    public void FadeDialogue()
    {
        dialogueDisplay.FadeAwayDialogue();
    }

    public void ClearDialogue()
    {
        currMessage.ClearPrio();
    }
}