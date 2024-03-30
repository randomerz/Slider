using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWheel : MonoBehaviour, ISavable
{
    [SerializeField] private STile stile;
    [SerializeField] private Meltable cog1;
    [SerializeField] private Meltable cog2;
    private bool powered = false;
    [SerializeField] private ElectricalNode powerNode;
    public Animator heaterAnimator;
    private bool inLavaStage = false;
    private int lavaCount = 0;
    private bool hasAddedLava = false;
    private bool hasMovedTile = false;
    private bool firstPower = false;
    private bool firstLavaPower = false;
    public WaterWheelAnimator animator;
    private bool hasOil;

    private void OnEnable() {
        SGridAnimator.OnSTileMoveStart += CheckMove;
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveStart -= CheckMove;
    }

    private void CheckMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
       // if(e.stile == stile)
            //ResetOnMove();
    }

    

    private void Update() {
        UpdatePower();
    }


    public void UpdatePower() {
        bool shouldPower = stile.x == 0 && stile.y > 1 && cog1.IsNotFrozenOrBroken() && cog2.IsNotFrozenOrBroken() && hasOil;
        powered = shouldPower;
        powerNode.StartSignal(shouldPower);
        if(!firstPower){
            firstPower = true;
            AudioManager.Play("Puzzle Complete");
        }
        if(powered && inLavaStage && !firstLavaPower){
            firstLavaPower = true;
            AudioManager.Play("Puzzle Complete");
        }
    }

    public void AddLava(int amount = 1)
    {
        if(!inLavaStage) return;
        
        lavaCount += amount;
        hasAddedLava = true;
        heaterAnimator.SetInteger("Lava",lavaCount);

        if(lavaCount >= 2){
            SetLavaComplete();
        }
    }

    public void ResetOnMove()
    {
        if(!inLavaStage) return;
        if(lavaCount > 1)
        {
           // cog2.RemoveLava();
            cog2.SetRefreezeOnTop(true);

        }
        if(lavaCount > 0)
        {
           // cog1.RemoveLava();
            cog1.SetRefreezeOnTop(true);
        }
        lavaCount = 0;
        heaterAnimator.SetInteger("Lava",lavaCount);
        hasMovedTile = true;
    }

    public void ActivateLavaStage(){
        inLavaStage = true;
        heaterAnimator.SetBool("Broken",false);
    }

    public void SetLavaComplete()
    {
        inLavaStage = false;
        //cog2.AddLava();
        cog2.Melt();
        cog2.SetRefreezeOnTop(false);
       // cog1.AddLava();
        cog1.Melt();
        cog1.SetRefreezeOnTop(false);
    }

    public bool IsDone(){
        return lavaCount > 1 && powered;
    }

    public void AddOil(bool fromSave = false){
        if(hasOil) return;
        if(!fromSave)
            AudioManager.Play("Hat Click");
        hasOil = true;
        animator.hasOil = true;
    }

    #region specs

    public void IsInPosition(Condition c) {
        c.SetSpec(stile.x == 0 && stile.y > 1);
    }

    public void IsNotInPosition(Condition c) {
        c.SetSpec(stile.x == 1 || stile.y < 2);
    }
    
    public void IsWorking(Condition c) {
        c.SetSpec(stile.x == 0 && stile.y > 1 && cog1.IsNotFrozenOrBroken() && cog2.IsNotFrozenOrBroken() && hasOil);
    }

    public void HasAddedLava(Condition c) {
        c.SetSpec(hasAddedLava);
    }

    public void ActiveLava(Condition c) {
        c.SetSpec(lavaCount > 0);
    }

    public void HasMovedTile(Condition c) {
        c.SetSpec(hasMovedTile);
    }

    public void IsDone(Condition c){
        c.SetSpec(lavaCount > 1 && powered);
    }

    public void InLavaMode(Condition c){
        c.SetSpec(inLavaStage);
    }

    public void HasOil(Condition c){
        c.SetSpec(hasOil);
    }

    #endregion

    public void Save()
    {
        SaveSystem.Current.SetBool("MountainWaterwheelHasOil", hasOil);
        SaveSystem.Current.SetBool("MountainWaterwheelLavaStage", inLavaStage);
        SaveSystem.Current.SetInt("MountainHeaterLavaCount", lavaCount);
        SaveSystem.Current.SetBool("MountainHeaterHasAddedLava", hasAddedLava);
        SaveSystem.Current.SetBool("MountainHeaterHasMovedTile", hasMovedTile);
        SaveSystem.Current.SetBool("MountainHeaterFirstPower", firstPower);
        SaveSystem.Current.SetBool("MountainHeaterFirstLavaPower", firstLavaPower);
    }

    public void Load(SaveProfile profile)
    {
        hasOil = profile.GetBool("MountainWaterwheelHasOil", hasOil);
        inLavaStage = profile.GetBool("MountainWaterwheelLavaStage", inLavaStage);
        lavaCount = profile.GetInt("MountainHeaterLavaCount", lavaCount);
        hasAddedLava = profile.GetBool("MountainHeaterHasAddedLava", hasAddedLava);
        hasMovedTile = profile.GetBool("MountainHeaterHasMovedTile", hasMovedTile);
        firstPower = profile.GetBool("MountainHeaterFirstPower", firstPower);
        firstLavaPower = profile.GetBool("MountainHeaterFirstLavaPower", firstPower);

        if(hasOil) AddOil();
        if(inLavaStage) ActivateLavaStage();
        AddLava(lavaCount);
    }

}
