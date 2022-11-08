using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuinsSymbols : MonoBehaviour
{
    public FlashWhite[] ruinSymbols = new FlashWhite[4];
    public SpriteRenderer ruinsHole;

    private void Start()
    {
        if (SaveSystem.Current.GetBool("villageHoleFilled"))
        {
            ruinsHole.enabled = false;
            SetSprites(true);
        }
    }

    public void SetSprites(bool value)
    {
        foreach (FlashWhite s in ruinSymbols)
        {
            s.SetSpriteActive(value);
        }
    }

    public void FlashSymbol(int index)
    {
        ruinSymbols[index].SetSpriteActive(true);
        ruinSymbols[index].Flash(3);
    }
}
