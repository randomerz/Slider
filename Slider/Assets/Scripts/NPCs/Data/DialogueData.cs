using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueData
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
