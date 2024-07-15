using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEnter : MonoBehaviour
{
    public bool onPlayerEnter = true;
    public UnityEvent onEnter;
    public UnityEvent onExit;

    private bool playerIsInTrigger = false;

    public void SetOnPlayerEnterActive(bool active, bool triggerIfAlreadyInside = true)
    {
        onPlayerEnter = active;
        if (onPlayerEnter && triggerIfAlreadyInside && playerIsInTrigger)
        {
            onEnter.Invoke();
        }
        else if (!onPlayerEnter && triggerIfAlreadyInside && playerIsInTrigger)
        {
            onExit.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerIsInTrigger = true;

            if (onPlayerEnter)
            {
                onEnter.Invoke();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerIsInTrigger = false;

            if (onPlayerEnter)
            {
                onExit.Invoke();
            }
        }
    }
}
