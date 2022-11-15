using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueData
{
    [TextArea(1, 4)]
    public string dialogue;

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
}
