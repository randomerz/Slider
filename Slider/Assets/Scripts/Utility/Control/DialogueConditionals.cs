using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[System.Serializable]
public class DialogueConditionals : Conditionals
{
    [Header("Dialogue Conditionals")]
    [TextArea(1, 4)]
    public string dialogue; //If you just have a single message, you can put it here, otherwise you can use dialogue chains for all the new functionality.
    public int priority;

    [FormerlySerializedAs("onDialogue")]    //Omg Thank You Unity :O:
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;
    public UnityEvent onDialogueChanged;


    private int currentprio = 0;

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

    //We might want to expand this to include all possible NPC actions (Dialogue, Walking, Starting Quests, etc.)
    public List<Dialogue> dialogueChain;
    public UnityEvent onDialogueChainExhausted;

    public List<NPC.NPCWalk> walks;

    public new bool CheckConditions()
    {
        foreach (Condition cond in conditions)
        {
            if (!cond.CheckCondition())
            {
                //priority is negative, so this dialogue will not be chosen.
                onFail?.Invoke();
                currentprio = -1 * priority;
                return false;
            }
        }
        onSuccess?.Invoke();
        currentprio = priority;
        return true;
    }

    public string GetDialogue()
    {
        return dialogue;
    }

    public string GetDialogueChain(int index)
    {
        if (index < 0 || index >= dialogueChain.Count)
        {
            Debug.LogError("Attempted to access nonexistent dialogue in chain.");
        }

        string dialogue = dialogueChain[index].dialogue;
        if (dialogueChain[index].waitUntilPlayerAction)
        {
            dialogue = string.Concat(dialogue, "<type>. . .</type>");
        }

        return dialogue;
    }

    public int GetPrio()
    {
        return currentprio;
    }

    public void OnDialogueStart()
    {
        onDialogueStart?.Invoke();   //This is for legacy purposes
    }

    public void OnDialogueEnd()
    {
        onDialogueEnd?.Invoke();   //This is for legacy purposes
    }

    public void OnDialogueChainStart(int index)
    {
        dialogueChain[index].onDialogueStart?.Invoke();
    }

    public void OnDialogueChainEnd(int index)
    {
        dialogueChain[index].onDialogueEnd?.Invoke();
    }

    //This can be used to start new events after an input when the dialogue is completely exhausted (For Ex: Starting the Chad Race)
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
