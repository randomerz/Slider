using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{
    public GameObject npc;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        npc.GetComponent<DialogueTrigger>().TriggerDialogue(npc);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        npc.GetComponent<DialogueManager>().FadeAwayDialogue();
    }
}
