using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWheel : MonoBehaviour
{
    [SerializeField] private STile stile;
    [SerializeField] private Meltable cog1;
    [SerializeField] private Meltable cog2;
    private bool powered = false;
    //[SerializeField] private ElectricalNode powerNode;
    public Animator heaterAnimator;
    private bool inLavaStage = false;
    private int lavaCount = 0;
    private bool hasAddedLava = false;
    private bool hasMovedTile = false;

    public void IsInPosition(Condition c) {
        c.SetSpec(stile.x == 0 && stile.y > 1);
    }

    public void IsNotInPosition(Condition c) {
        c.SetSpec(stile.x == 1 || stile.y < 2);
    }
    
    public void IsWorking(Condition c) {
        c.SetSpec(stile.x == 0 && stile.y > 1 && cog1.IsNotFrozenOrBroken() && cog2.IsNotFrozenOrBroken());
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

    private void Update() {
        UpdatePower(); //C: Temporary
    }


    public void UpdatePower() {
        bool shouldPower = stile.x == 0 && stile.y > 1 && cog1.IsNotFrozenOrBroken() && cog2.IsNotFrozenOrBroken();
        powered = shouldPower;
        //powerNode.StartSignal(shouldPower);
    }

    public void AddLava()
    {
        lavaCount++;
        hasAddedLava = true;
        heaterAnimator.SetInteger("Lava",lavaCount);
        if(lavaCount == 1) {
            cog1.AddLava();
            cog1.Melt();
        }
        if(lavaCount == 2){
            cog2.AddLava();
            cog2.Melt();
        }
    }

    public void ResetOnMove()
    {
        if(!inLavaStage) return;
        if(lavaCount > 1)
            cog2.RemoveLava();
        if(lavaCount > 0)
            cog1.RemoveLava();
        lavaCount = 0;
        heaterAnimator.SetInteger("Lava",lavaCount);
        hasMovedTile = true;
    }

    public void ActivateLavaStage(){
        inLavaStage = true;
    }
}
