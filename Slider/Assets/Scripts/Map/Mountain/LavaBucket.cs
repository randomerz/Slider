using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaBucket : MonoBehaviour
{
    [SerializeField] private SpriteSwapper spriteSwapper;

    private bool hasLava = false;

    public void FillBucket()
    {
        spriteSwapper.TurnOn();
        hasLava = true;
    }

    public void IsEmpty(Condition c) {
        c.SetSpec(!hasLava);
    }

    public void IsFull(Condition c) {
        c.SetSpec(hasLava);
    }
}
