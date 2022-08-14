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
    public UnityEvent onFix;

    public int numLavaSources = 0;
    public bool anchorBroken = false;
    public bool canBreakWithAnchor = true;
    private int numTimesBroken = 0;
    public STile sTile;

    private void OnEnable() {
        MountainSGridAnimator.OnSTileMoveEnd += CheckFreezeOnMoveEnd;
        sTile = GetComponentInParent<MountainSTile>();
    }

    private void OnDisable() {
        MountainSGridAnimator.OnSTileMoveEnd -= CheckFreezeOnMoveEnd;
    }

    public void CheckFreezeOnMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if(sTile == null || e.stile == null)
            return;
        if(sTile != null && e.stile.islandId == sTile.islandId)
            Debug.Log("amogus");
        if(sTile != null && e.stile.islandId == sTile.islandId && sTile.y > 1 && !isFrozen && numLavaSources <= 0 && !anchorBroken)
            Freeze();
    }

    public virtual void Melt(bool usingAnchor = false)
    {
        Debug.Log("melt");
        if(usingAnchor && canBreakWithAnchor && !anchorBroken)
        {
            anchorBroken = true;
            numTimesBroken++;
        }
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

    public void AddLava()
    {
        numLavaSources++;
        Melt();
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

    public bool IsNotFrozenOrBroken() {
        return(!isFrozen && !anchorBroken);
    }

    public void HasBeenBrokenMultipleTimes(Condition c){
        c.SetSpec(numTimesBroken > 1);
    }

    public void Freeze()
    {
        Debug.Log("Freeze");
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
        onFix.Invoke();
    }
}
