using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEnter : MonoBehaviour
{
    public bool watchingForPlayer { get; private set; } = true;
    private bool playerIsInTrigger = false;

    public UnityEvent onEnter;
    public UnityEvent onExit;

    public void EnableWatchForPlayer(bool enabled, bool triggerIfAlreadyInside = true)
    {
        watchingForPlayer = enabled;

        if (enabled && triggerIfAlreadyInside && playerIsInTrigger)
        {
            onEnter.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player")
        {
            return;
        }

        playerIsInTrigger = true;

        if (watchingForPlayer)
        {
            onEnter.Invoke();
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag != "Player")
        {
            return;
        }

        playerIsInTrigger = false;

        if (watchingForPlayer)
        {
            onExit.Invoke();
        }
    }
}
