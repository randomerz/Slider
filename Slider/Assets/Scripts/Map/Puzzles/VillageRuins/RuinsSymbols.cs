using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuinsSymbols : MonoBehaviour
{
    public FlashWhiteSprite[] ruinSymbols = new FlashWhiteSprite[4];
    public SpriteRenderer ruinsHole;

    private void Start()
    {
        if (SaveSystem.Current.GetBool("villageCompletion")) // check against "villageCompletion" instead of "villageHoldFilled" to avoid a softlock
        {
            ruinsHole.enabled = false;
            SetSprites(true);
        }
    }

    public void SetSprites(bool value)
    {
        foreach (FlashWhiteSprite s in ruinSymbols)
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
