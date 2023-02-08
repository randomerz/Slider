using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[System.Serializable]
public class NPCConditionals
{
    [HideInInspector] public string name;
    public List<Condition> conditions;

    [Header("Dialogue")]
    public bool alwaysStartFromBeginning;
    public List<DialogueData> dialogueChain;
    [HideInInspector] public bool isDialogueChainExhausted;

    // Animator parts
    [Tooltip("Name of NPC animation to play when conditional is entered.")]
    public string animationOnEnter;
    // Feel free to uncomment these if you want to use them (and in NPCDialogueContext.TryDialogueChainExhausted() and NPCConditionalsDrawer.ANIMATOR_PROPERTY_NAMES)
    // [Tooltip("Name of NPC animation to play when conditional is exhausted.")] 
    // public string animationOnExhaust;
    [Tooltip("Name of NPC emote to display when conditional is entered.")]
    public NPCEmotes.Emotes emoteOnEnter;
    // [Tooltip("Name of NPC emote to display when conditional is exhausted.")]
    // public NPCEmotes.Emotes emoteOnExhaust;

    // Programmy parts
    [FormerlySerializedAs("onDialogueChanged")]
    public UnityEvent onConditionalEnter;
    public UnityEvent onDialogueChainExhausted;
    public List<NPCWalkData> walks;

    [HideInInspector] public int priority;
    private int currentprio = 0;

    // Editor tools
    [HideInInspector] public bool editorIsAnimatorUnfolded;
    [HideInInspector] public bool editorIsProgrammerUnfolded;
    
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

    public void SetNPCAnimatorController(NPCAnimatorController controller)
    {
        Debug.Log("updating dd");
        foreach (DialogueData data in dialogueChain)
        {
            // data.npcAnimatorController = controller;
            // data.asn = controller.animationStateNames;
        }
    }
}
