using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{
    public GameObject npc;
    public bool firstTimeFezziwigCheck = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            npc.GetComponent<DialogueTrigger>().TriggerDialogue(npc);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (!firstTimeFezziwigCheck && npc.GetComponent<NPC>().characterName == "Fezziwig")
            {
                //ADD NEW THING HERE
                firstTimeFezziwigCheck = true;
                return;
            }
            npc.GetComponent<DialogueManager>().FadeAwayDialogue();
        }
    }
}
