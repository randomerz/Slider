using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class CreditsTile : MonoBehaviour
{
    public UnityEvent OnStartSlideIn;
    public UnityEvent OnEndSlideIn;
    public UnityEvent OnStartSlideOut;
    public List<DelayedEvent> events;

    [Serializable]
    public class DelayedEvent
    {
        public UnityEvent Event;
        public float Delay;
    }
}
