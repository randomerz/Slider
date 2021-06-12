using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public NPC npc;

    //Call to dialogueManager function
    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueManager>().DisplayDialogues(dialogue, npc.GetCurrentDialogueNumber());
    }
}