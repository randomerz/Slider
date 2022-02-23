using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEnter : MonoBehaviour
{
    public bool onPlayerEnter = true;
    public UnityEvent onEnter;
    public UnityEvent onExit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (onPlayerEnter && other.tag == "Player")
        {
            onEnter.Invoke();
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (onPlayerEnter && other.tag == "Player")
        {
            onExit.Invoke();
        }
    }
}
