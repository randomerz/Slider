using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[System.Serializable]
public class NPCAction
{
    [System.Serializable]
    public class Dialogue
    {
        [TextArea(1, 4)]
        public string dialogue;

        public float delayAfterFinishedTyping = 0.5f;
        public bool waitUntilPlayerAction;  //Player has to press e to continue.
        public bool doNotRepeatAfterTriggered;
        public bool dontInterrupt;

        public UnityEvent onDialogueStart;
        public UnityEvent onDialogueEnd;
    }

    public List<Condition> conditions;

    [Header("Dialogue Conditionals")]
    public List<Dialogue> dialogueChain;
    public bool alwaysStartFromBeginning;
    public UnityEvent onDialogueChanged;
    public UnityEvent onDialogueChainExhausted;

    public List<NPC.NPCWalk> walks;

    [HideInInspector] public int priority;
    private int currentprio = 0;

    public bool CheckConditions()
    {
        foreach (Condition cond in conditions)
        {
            if (!cond.CheckCondition())
            {
                currentprio = -1 * priority;    //priority is negative so that the dialogue will not be chosen.
                return false;
            }
        }
        currentprio = priority;
        return true;
    }

    public string GetDialogueString(int index)
    {
        if (index < 0 || index >= dialogueChain.Count)
        {
            Debug.LogError("Attempted to access nonexistent dialogue in chain.");
        }

        string dialogue = dialogueChain[index].dialogue;
        if (dialogueChain[index].waitUntilPlayerAction)
        {
            dialogue = string.Concat(dialogue, "<type> ...</type>");
        }

        return dialogue;
    }

    public int GetPrio()
    {
        return currentprio;
    }

    public void OnDialogueChainStart(int index)
    {
        if (index < 0 || index >= dialogueChain.Count)
        {
            Debug.LogError("Attempted to access nonexistent dialogue in chain.");
        }
        dialogueChain[index].onDialogueStart?.Invoke();
    }

    public void OnDialogueChainEnd(int index)
    {
        if (index < 0 || index >= dialogueChain.Count)
        {
            Debug.LogError("Attempted to access nonexistent dialogue in chain.");
        }
        dialogueChain[index].onDialogueEnd?.Invoke();
    }

    public void OnDialogueChainExhausted()
    {
        onDialogueChainExhausted?.Invoke();
    }

    public void SetPrio(int p)
    {
        priority = p;
    }
    
    public void KillDialogue()
    {
        priority = 0;
    }
}
