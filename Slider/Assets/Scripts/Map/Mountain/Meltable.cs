using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;


public class Meltable : FlashWhiteSprite, ISavable
{
    [Header("Sprites")]
   // [SerializeField] private Animator animator; C: Will switch over later
    
    [SerializeField] private Sprite frozenSprite;
    [SerializeField] private Sprite meltedSprite;
    [SerializeField] private Sprite anchorBrokenSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite newFrozenSprite;


    [Header("Visuals")]
    [Tooltip("At the end of the refreeze time, when this curve is > 1, the object sprite will flash frozen")]
    [SerializeField] private AnimationCurve blinkCurve;

    [Header("Events")]
    public UnityEvent onMelt;
    public UnityEvent onFreeze;
    public UnityEvent onBreak;
    public UnityEvent onFix;


    [Header("Properties")]
    [SerializeField] private bool canBreakWithAnchor = true;
    [SerializeField] private bool canBreakWhenNotFrozen = false;
    [SerializeField] private bool breakToMelted = false;
    [SerializeField] private bool refreezeOnTop = true;
    [SerializeField] private bool refreezeFromBroken = false;
    [SerializeField] private bool fixBackToFrozen = false;
    [SerializeField] private float freezeTime = 5.0f;

    public enum MeltableState
    {
        FROZEN,
        MELTED,
        BROKEN
    }

    public MeltableState state;

    public int numLavaSources = 0;
    private int numTimesBroken = 0;
    private STile sTile;

    private float currFreezeTime;
    private float blinkTime;
    private bool hasFixed = false;

    public List<Lava> lavasources = new();

    private void OnEnable() {
        sTile = GetComponentInParent<MountainSTile>();
        SGridAnimator.OnSTileMoveEndEarly += CheckFreezeOnMoveEnd; //C: Has to be early + delay or else tile in args is null
        currFreezeTime = freezeTime;
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveEndEarly -= CheckFreezeOnMoveEnd;
    }

    private void Start() {
        if (blinkCurve.length > 0)
            blinkTime = blinkCurve[blinkCurve.length - 1].time;
    }


    private void Update() {
        if(Time.timeScale != 0 && CheckFreeze())
        {
            currFreezeTime -= Time.deltaTime;
            if(state != MeltableState.FROZEN && currFreezeTime < blinkTime && currFreezeTime > 0)
                ToggleBlinkSprite(blinkTime - currFreezeTime);
            if(currFreezeTime < 0)
                Freeze();
        }
    }

    private void ToggleBlinkSprite(float time)
    {
        float val = blinkCurve.Evaluate(time);
        if(val > 1) {
            if(spriteRenderer)
                spriteRenderer.sprite = frozenSprite;
        } else {
            if(spriteRenderer)
                spriteRenderer.sprite = (state == MeltableState.BROKEN) ? anchorBrokenSprite : meltedSprite;
        }
    }

    public void SetCanBreakWithAnchor(bool value)
    {
        canBreakWithAnchor = value;
    }

    public void CheckFreezeOnMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if(e.stile == null)
            return;
        if(e.stile == sTile)
            StartCoroutine(WaitThenCheckFreeze());
    }

    /*C: We can't check for freezing directly at the end of the move because even though OnSTileMoveEndEarly and OnSTileMoveEnd
         are both called after the tile is moved into place, it seems as if the colliders don't update quickly enough.
         (might have something do with the fact that physics calculations happen early on, so maybe it isn't updated until next frame?)
         Either way after copious amounts of testing this combination of OnSTileMoveEndEarly with this tiny delay
         makes refreezing work as intended. 
    */
    IEnumerator WaitThenCheckFreeze() 
    {
        yield return new WaitForSeconds(0.03f);
        if(CheckFreeze())
                Freeze();
    }

    public bool CheckFreeze()
    {
        return (refreezeOnTop && ((sTile != null && sTile.y > 1) || transform.position.y > 62.5) && (state != MeltableState.BROKEN || refreezeFromBroken) && numLavaSources <= 0);
    }

    public void FlashThenBreak() {
        Action checkbreak = () => {Break();};
        Flash(1, checkbreak);
    }

    public void Break(bool fromLoad = false) {
        if(fromLoad || ((state == MeltableState.FROZEN || canBreakWhenNotFrozen) && canBreakWithAnchor && state != MeltableState.BROKEN))
        {
            if(breakToMelted)
                Melt(true);
            else
            {
                state = MeltableState.BROKEN;
                numTimesBroken++;
                if(spriteRenderer)
                    spriteRenderer.sprite = anchorBrokenSprite;
                onBreak?.Invoke();
                currFreezeTime = freezeTime;
            }
        }
    }

    public void Melt(bool fromLoad = false)
    {
        if(fromLoad || (state == MeltableState.FROZEN && numLavaSources > 0)) 
        {
            state = MeltableState.MELTED;
            if(spriteRenderer)
                spriteRenderer.sprite = meltedSprite;
            onMelt?.Invoke();
            currFreezeTime = freezeTime;
        }
    }

    public void Freeze(bool fromLoad = false)
    {
        if(fromLoad || state != MeltableState.FROZEN)
        {
            state = MeltableState.FROZEN;
            if(spriteRenderer)
                spriteRenderer.sprite = hasFixed && newFrozenSprite != null ? newFrozenSprite : frozenSprite;
            onFreeze?.Invoke();
            currFreezeTime = freezeTime;
        }
    }

    public void Fix()
    {
        if(state == MeltableState.BROKEN) 
        {
            state = MeltableState.MELTED;
            hasFixed = true;
           // animator.SetBool("Broken", false);
            if(fixBackToFrozen)
                Freeze();
            else if(spriteRenderer)
                spriteRenderer.sprite = meltedSprite;
            onFix?.Invoke();
        }
    }

    public void RemoveLava(Lava lava)
    {
        lavasources.Remove(lava);
        numLavaSources--;
    }

    public void AddLava(Lava lava)
    {
        if(!lavasources.Contains(lava))
                lavasources.Add(lava);
        numLavaSources++;
        Melt();
    }

    public void SetRefreezeOnTop(bool value){
        refreezeOnTop = value;
    }

    public void SetBreakToMelted(bool value){
        breakToMelted = value;
    }

    public void SetFixBackToFrozen(bool value){
        fixBackToFrozen = value;
    }

    public void ChangeFrozenSprite(){
        spriteRenderer.sprite = newFrozenSprite;
    }

    public void Save()
    {
        SaveSystem.Current.SetInt(gameObject.name + gameObject.transform.parent.name + "MeltState", ((int)state));
        SaveSystem.Current.SetBool(gameObject.name + gameObject.transform.parent.name + "HasFixed", hasFixed);
    }

    public void Load(SaveProfile profile)
    {
        hasFixed = SaveSystem.Current.GetBool(gameObject.name + gameObject.transform.parent.name + "HasFixed");
        state = (MeltableState)profile.GetInt(gameObject.name + gameObject.transform.parent.name + "MeltState");
        if(state == MeltableState.FROZEN)
            Freeze(true);
        else if(state == MeltableState.BROKEN)
            Break(true);
        else
            Melt(true);
    }

    public void IsFrozen(Condition c) {
        c.SetSpec(state == MeltableState.FROZEN);
    }

    public void IsBroken(Condition c) {
        c.SetSpec(state == MeltableState.BROKEN);
    }

    public void IsNotFrozen(Condition c) {
        c.SetSpec(state != MeltableState.FROZEN);
    }

    public void IsNotFrozenOrBroken(Condition c) {
        c.SetSpec(state == MeltableState.MELTED);
    }

    public bool IsNotFrozenOrBroken() {
        return(state == MeltableState.MELTED);
    }

    public void HasBeenBrokenMultipleTimes(Condition c){
        c.SetSpec(numTimesBroken > 1);
    }
}
