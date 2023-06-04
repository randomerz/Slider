using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaBucket : MonoBehaviour, ISavable
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

    public void Load(SaveProfile profile)
    {
        hasLava = profile.GetBool("MountainLavaBucket", hasLava);
        if(hasLava)
            FillBucket();
    }

    public void Save()
    {
        SaveSystem.Current.SetBool("MountainLavaBucket", hasLava);
    }
}
