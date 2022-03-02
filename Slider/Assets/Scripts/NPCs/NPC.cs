using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public string characterName;
    public List<DialogueConditionals> dconds;

    [SerializeField] private DialogueDisplay dialogueDisplay;

    private int currMessage;

    // might need optimizing
    void Update()
    {
        foreach (DialogueConditionals d in dconds)
        {
            d.CheckConditions();
        }
        int newDialogue = CurrentDialogue();
        if (currMessage != newDialogue)
        {
            currMessage = newDialogue;
            dialogueDisplay.NewMessagePing();
        }
    }

    public int CurrentDialogue()
    {
        int curr = -1;
        int max = 0;
        for (int i = 0; i< dconds.Count; i++)
        {
            if (dconds[i].GetPrio() > max)
            {
                curr = i;
                max = dconds[i].GetPrio();
            }
        }
        if (curr == -1)
        {
            Debug.LogError("No suitable dialogue can be displayed!");
        }
        return curr;
    }
    public void TriggerDialogue()
    {
        dconds[currMessage].OnDialogue();
        dialogueDisplay.DisplaySentence(dconds[currMessage].GetDialogue());
    }

    public void FadeDialogue()
    {
        dialogueDisplay.FadeAwayDialogue();
    }

    public void ClearDialogue()
    {
        dconds[currMessage].KillDialogue();
    }

    public void SetNextDialogue()
    {
        if (currMessage < dconds.Count - 1)
        {
            dconds[currMessage+1].SetPrio(dconds[currMessage].GetPrio());
        }
    }

    public void Teleport(Transform trans)
    {
        transform.position = trans.position;
        transform.parent = trans.parent;
    }
}