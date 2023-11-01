using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaBucket : MonoBehaviour, ISavable
{
    [SerializeField] private SpriteSwapper spriteSwapper;
    [SerializeField] private GameObject particles;

    private STile sTile;

    private bool hasLava = false;

    private void Awake() {
        sTile = GetComponentInParent<STile>();
    }

    public void FillBucket(bool fromSave = false)
    {
        if(!fromSave && !hasLava)
        {
            AudioManager.Play("Puzzle Complete");
        }
        spriteSwapper.TurnOn();
        particles.SetActive(true);
        hasLava = true;
    }

    public void IsEmpty(Condition c) {
        c.SetSpec(!hasLava);
    }

    public void IsFull(Condition c) {
        c.SetSpec(hasLava);
    }

    public void IsInValidPos(Condition c) {
        c.SetSpec(sTile != null && sTile.y % 2 == 0);
    }

    public void Load(SaveProfile profile)
    {
        hasLava = profile.GetBool("MountainLavaBucket", hasLava);
        if(hasLava)
            FillBucket(true);
    }

    public void Save()
    {
        SaveSystem.Current.SetBool("MountainLavaBucket", hasLava);
    }
}
