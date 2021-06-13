using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    //public GameObject npc;

    //Call to dialogueManager function
    public void TriggerDialogue(GameObject npc)
    {
        npc.GetComponent<DialogueManager>().DisplayDialogues(dialogue, npc.GetComponent<NPC>().GetCurrentDialogueNumber());
    }
}