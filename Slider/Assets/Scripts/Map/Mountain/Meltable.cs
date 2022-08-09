using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Meltable : MonoBehaviour
{
    public Sprite frozenSprite; //C: switch to animation later maybe? but that doesn't sound fun
    public Sprite meltedSprite;
    public Sprite anchorBrokenSprite;
    public SpriteRenderer spriteRenderer;

    public bool isFrozen = true;
  //  private bool canBeChanged = true;

    public UnityEvent onMelt;
    public UnityEvent onFreeze;

    public int numLavaSources = 0;
    public bool anchorBroken = false;
    public bool canBreakWithAnchor = true;

    // Update is called once per frame
    void Update()
    {
        if(this.transform.position.y > 62 && !isFrozen && numLavaSources <= 0 && !anchorBroken)
            Freeze();
    }

    public virtual void Melt(bool usingAnchor = false)
    {
        if(usingAnchor && canBreakWithAnchor)
        {
            anchorBroken = true;
        }
        else
            numLavaSources++;
        if(isFrozen && (anchorBroken || numLavaSources > 0)) //C: the second check is pointless but maybe wacky things could happen
        {
            if (spriteRenderer)
            {
                if(anchorBrokenSprite != null && anchorBroken)
                    spriteRenderer.sprite = anchorBrokenSprite;
                else
                    spriteRenderer.sprite = meltedSprite;
            }
            isFrozen = false;
            onMelt.Invoke();
        }
    }

   // public void SetCanBeChanged(bool value)
  //  {
   //     canBeChanged = value;
  //  }

    public void RemoveLava()
    {
        numLavaSources--;
    }

    public void RemoveAnchor()
    {
        anchorBroken = false;
    }

    public void IsFrozen(Condition c) {
        c.SetSpec(isFrozen);
    }

    public void IsNotFrozen(Condition c) {
        c.SetSpec(!isFrozen);
    }

    public void IsNotFrozenOrBroken(Condition c) {
        c.SetSpec(!isFrozen && !anchorBroken);
    }

    public void Freeze()
    {
        if (spriteRenderer)
            spriteRenderer.sprite = frozenSprite;
        isFrozen = true;
        onFreeze.Invoke();
    }

    public void Fix()
    {
        anchorBroken = false;
        if (spriteRenderer)
            spriteRenderer.sprite = meltedSprite;
    }
}
