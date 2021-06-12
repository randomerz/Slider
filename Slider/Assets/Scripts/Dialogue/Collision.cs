using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{
    public GameObject DialogueManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        FindObjectOfType<DialogueTrigger>().TriggerDialogue();
    }
}
