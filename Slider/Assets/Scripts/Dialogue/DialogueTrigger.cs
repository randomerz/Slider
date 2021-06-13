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
        Debug.Log(npc.GetComponent<NPC>().characterName);
        foreach (string str in dialogue.dialogues)
        {
            Debug.Log(str);
        }
        npc.GetComponent<DialogueManager>().DisplayDialogues(dialogue, npc.GetComponent<NPC>().GetCurrentDialogueNumber());
    }
}