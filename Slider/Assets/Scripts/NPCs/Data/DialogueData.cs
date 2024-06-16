using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueData
{
    public string DialoguePreferLocalized
    {
        get
        {
            if (dialogueLocalized != null)
            {
                return dialogueLocalized;
            }
            else
            {
                return dialogue;
            }
        }
    }

    public string DialogueLocalized
    {
        set => dialogueLocalized = value;
    }
    
    private string dialogueLocalized = null;
    
    [TextArea(1, 4)]
    public string dialogue;

    // Animator Parts
    [Tooltip("Name of NPC animation to play when dialogue starts.")]
    public string animationOnStart;
    [Tooltip("Name of NPC animation to play when you leave the NPC.")]
    public string animationOnLeave;
    [Tooltip("Name of NPC emote to display when dialogue starts.")]
    public NPCEmotes.Emotes emoteOnStart;
    [Tooltip("Name of NPC emote to display when you leave the NPC.")]
    public NPCEmotes.Emotes emoteOnLeave;

    // Programmer Parts
    [Tooltip("Only applies when waitUntilPlayerAction and advanceDialogueManually are false, and != 0")]
    public float delayAfterFinishedTyping = 0.5f;
    [Tooltip("Player has to press e to continue.")] 
    public bool waitUntilPlayerAction;
    [Tooltip("You have to call AdvanceDialogueChain() (usually through an event) to advance it.")]
    public bool advanceDialogueManually; 

    [Tooltip("OnDialogueStart and OnDialogueEnd will only run once.")]
    public bool doNotRepeatEvents;
    [Tooltip("Exiting the trigger does not end the dialogue. It will always run to completion (use for cutscenes and such).")]
    public bool dontInterrupt;

    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    // Editor tools
    [HideInInspector] public bool editorIsAnimatorUnfolded;
    [HideInInspector] public bool editorIsProgrammerUnfolded;
}
