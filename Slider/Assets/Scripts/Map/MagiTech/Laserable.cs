using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Laserable : MonoBehaviour
{
    public enum LaserInteractionType
    {
        Reflect,
        Portal,
        Absorb
    }

    public LaserInteractionType laserInteractionType;
    [Tooltip("ONLY FOR MIRRORS. Check if mirror faces backwards")]
    public bool flipDirection;
    public UnityEvent OnLasered;
}
