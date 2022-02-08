using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{
    public GameObject npc;

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
            if (!NPCManager.firstTimeFezziwigCheck && npc.GetComponent<NPC>().characterName == "Fezziwig")
            {
                NPCManager.firstTimeFezziwigCheck = true;
                //EightPuzzle.ShuffleBoard();
                // int[,] shuffledPuzzle = new int[3, 3] { { 7, 0, 1 },
                //                                         { 6, 4, 8 },
                //                                         { 5, 3, 2 } };
                // SGrid.current.SetGrid(shuffledPuzzle);
                VillageGrid.instance.ShufflePuzzle();
                npc.GetComponent<DialogueManager>().FadeAwayDialogue();
                return;
            }
            npc.GetComponent<DialogueManager>().FadeAwayDialogue();
        }
    }
}
