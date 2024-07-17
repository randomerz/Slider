using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class OnTriggerEnter : MonoBehaviour
{
    public bool onPlayerEnter = true;
    [FormerlySerializedAs("onItemAnchorEnter")]
    public bool onAnchorEnter = false;
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
        if (other.CompareTag("Player"))
        {
            playerIsInTrigger = true;

            if (onPlayerEnter)
            {
                onEnter.Invoke();
            }
        }
        if (other.CompareTag("Item"))
        {
            if (onAnchorEnter && other.GetComponent<Anchor>() != null)
            {
                onEnter.Invoke();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInTrigger = false;

            if (onPlayerEnter)
            {
                onExit.Invoke();
            }
        }
        if (other.CompareTag("Item"))
        {
            if (onAnchorEnter && other.GetComponent<Anchor>() != null)
            {
                onExit.Invoke();
            }
        }
    }
}
