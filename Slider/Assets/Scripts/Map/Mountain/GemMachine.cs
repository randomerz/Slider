using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemMachine : MonoBehaviour, ISavable
{
    private int numGems;
    private STile sTile;
    private bool isPowered;
    private bool isDone;

    private void OnEnable() {
        SGridAnimator.OnSTileMoveStart += CheckMove;
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveStart -= CheckMove;
    }

    private void CheckMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if(e.stile == sTile)
            ResetGems();
    }

    private void Start() {
        sTile = GetComponentInParent<STile>();
    }

    public void addGem(){
        if(!isPowered)
            return;
        numGems++;
        if(numGems == 2){
            isDone = true;
            SGrid.Current.EnableStile(8);
        }
    }

    public void RemoveGem(){
        numGems--;
    }

    public void ResetGems(){
        numGems = 0;
    }

    public void SetIsPowered(bool value){
        isPowered = value;
    }

    public void Save(){
        SaveSystem.Current.SetString("MountainNumGems", numGems.ToString());
    }

    public void Load(SaveProfile profile)
    {
        numGems = int.Parse(profile.GetString("MountainNumGems"));
    }

    public void CheckHasCrystals(Condition c){
        c.SetSpec(isDone);
    }
}
