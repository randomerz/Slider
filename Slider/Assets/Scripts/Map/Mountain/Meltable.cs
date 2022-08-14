using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Meltable : MonoBehaviour
{
    [Header("Sprites")]
   // [SerializeField] private Animator animator; C: Will switch over later
    
    public Sprite frozenSprite;
    public Sprite meltedSprite;
    public Sprite anchorBrokenSprite;
    public SpriteRenderer spriteRenderer;

    public bool isFrozen = true;

    [Header("Events")]
    public UnityEvent onMelt;
    public UnityEvent onFreeze;
    public UnityEvent onBreak;
    public UnityEvent onFix;


    [Header("Properties")]
    [SerializeField] private  bool canBreakWithAnchor = true;
    [SerializeField] private  bool canBreakWhenNotFrozen = false;
    [SerializeField] private  bool refreezeOnTop = true;
    [SerializeField] private  bool refreezeFromBroken = false;
    [SerializeField] private bool fixBackToFrozen = false;


    public int numLavaSources = 0;
    public bool anchorBroken = false;
    private int numTimesBroken = 0;
    private STile sTile;

    private void OnEnable() {
        sTile = GetComponentInParent<MountainSTile>();
        Debug.Log(gameObject.name + " " + sTile.islandId);
        MountainSGridAnimator.OnSTileMoveEnd += CheckFreezeOnMoveEnd;
    }

    private void OnDisable() {
        MountainSGridAnimator.OnSTileMoveEnd -= CheckFreezeOnMoveEnd;
    }

    public void CheckFreezeOnMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if(sTile == null || e.stile == null)
            return;
        if(sTile != null && e.stile.islandId == sTile.islandId)
        {
            Debug.Log("same stile " + gameObject.name);
            if(CheckFreeze())
                Freeze();
        }
    }

    public bool CheckFreeze()
    {
        return (!refreezeOnTop || sTile.y > 1) && (!anchorBroken || refreezeFromBroken) && numLavaSources <= 0;
    }

    public void Break() {
        if((isFrozen || canBreakWhenNotFrozen) && canBreakWithAnchor && !anchorBroken)
        {
            anchorBroken = true;
            numTimesBroken++;
            isFrozen = false;
           // animator.SetBool("Broken", true);
            if(spriteRenderer)
                spriteRenderer.sprite = anchorBrokenSprite;
            onBreak.Invoke();
        }
    }

    public void Melt()
    {
        if(isFrozen && numLavaSources > 0) 
        {
            isFrozen = false;
            if(spriteRenderer)
                spriteRenderer.sprite = meltedSprite;
            onMelt.Invoke();
            Debug.Log("Melt "+ gameObject.name);
        }
    }

    public void Freeze()
    {
        if(!isFrozen)
        {
            isFrozen = true;
            if(spriteRenderer)
                spriteRenderer.sprite = frozenSprite;
            onFreeze.Invoke();
            Debug.Log("Freeze " + gameObject.name);
        }
    }

    public void Fix()
    {
        if(anchorBroken) 
        {
            anchorBroken = false;
           // animator.SetBool("Broken", false);
            if(fixBackToFrozen)
                Freeze();
            else if(spriteRenderer)
                spriteRenderer.sprite = frozenSprite;
            onFix.Invoke();
        }
    }

    public void RemoveLava()
    {
        numLavaSources--;
    }

    public void AddLava()
    {
        numLavaSources++;
        Melt();
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
}
