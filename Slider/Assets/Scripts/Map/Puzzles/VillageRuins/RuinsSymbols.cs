using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuinsSymbols : MonoBehaviour
{
    public FlashWhite[] ruinSymbols = new FlashWhite[4];

    private void Start()
    {
        if (SaveSystem.Current.GetBool("villageCompletion"))
        {
            foreach (FlashWhite s in ruinSymbols)
            {
                s.SetSpriteActive(true);
            }
        }
    }

    public void FlashSymbol(int index)
    {
        ruinSymbols[index].SetSpriteActive(true);
        ruinSymbols[index].Flash(3);
    }
}
