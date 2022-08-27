using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemMachine : MonoBehaviour, ISavable
{
    private int numGems;
    private STile sTile;
    private bool isPowered;
    private bool isDone;
    private bool isBroken = true;
    public GameObject gemChecker;

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

    private void Update() {
        gemChecker.SetActive(isPowered && !isBroken);
    }

    public void addGem(){
        if(!isPowered || isBroken)
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

    public void Fix()
    {
        isBroken = false;
    }

    public void Save(){
        SaveSystem.Current.SetString("mountainNumGems", numGems.ToString());
        SaveSystem.Current.SetBool("mountainGemMachineBroken", isBroken);
    }

    public void Load(SaveProfile profile)
    {
        string temp = profile.GetString("mountainNumGems");
        if (temp.Equals("mountainNumGems"))
            numGems = 0;
        else
            numGems = int.Parse(profile.GetString("mountainNumGems"));
        isBroken = profile.GetBool("mountainGemMachineBroken", true);
    }

    public void CheckHasCrystals(Condition c){
        c.SetSpec(isDone);
    }

    public void CheckIsPowered(Condition c){
        c.SetSpec(isPowered);
    }

    public void CheckIsFixed(Condition c){
        c.SetSpec(!isBroken);
    }
}
