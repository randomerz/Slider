using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueData
{
    [TextArea(1, 4)]
    public string dialogue;


    public float delayAfterFinishedTyping = 0.5f;   //Only applies when waitUntilPlayerAction and advanceDialogueManually are false
    public bool waitUntilPlayerAction;  //Player has to press e to continue.
    public bool advanceDialogueManually;    //You have to call AdvanceDialogueChain() (usually through an event) to advance it.

    public bool doNotRepeatEvents;  //OnDialogueStart and OnDialogueEnd will only run once.
    public bool dontInterrupt;  //Exiting the trigger does not end the dialogue. It will always run to completion (use for cutscenes and such).

    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;
}
