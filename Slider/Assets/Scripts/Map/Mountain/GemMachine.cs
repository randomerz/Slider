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
    public Animator animator;

    public GameObject brokenObj;
    public GameObject fixedObj;
    public PipeLiquid pipeLiquid;

    public enum GemMachinePhase
    {
        INITIAL,
        BROKEN,
        FIXED,
        FULLY_GOOED
    }

    public GemMachinePhase phase = GemMachinePhase.INITIAL;

    private void Start() {
        sTile = GetComponentInParent<STile>();
    }

    private void Update() {
        gemChecker.SetActive(isPowered && !isBroken);
    }

    private void OnEnable() {
        SGridAnimator.OnSTileMoveStart += CheckMove;
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveStart -= CheckMove;
    }

    private void CheckMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile == sTile)
            ResetGems();
    }

    public void AddGem(){
        if(!isPowered || isBroken)
        {
            Debug.LogWarning("Gem machine took gem when it should not");
            return;
        }

        animator.Play("AbsorbGem");

        switch(phase)
        {
            case GemMachinePhase.INITIAL:
                print("take gem and blow up generator");
                break;
            case GemMachinePhase.BROKEN:
                print("this should not happen");
                break;
            case GemMachinePhase.FIXED:
                print("add gem");
                break;
            case GemMachinePhase.FULLY_GOOED:
                print("idk yet lol");
                break;
        }
        
        // numGems++;
        // if(numGems == 2){
        //     AudioManager.Play("Puzzle Complete");
        //     isDone = true;
        //     EnableGoo();
        // }
    }

    private void IntialGemAbsorb()
    {

    }

    private void AddGemToContainer()
    {

    }

    public void RemoveGem(){
        numGems--;
    }

    public void ResetGems(){
        numGems = 0;
    }

    public void OnEndAbsorb(){
        animator.Play("Empty");
    }

    public void SetIsPowered(bool value){
        isPowered = value;
    }

    public void Fix()
    {
        isBroken = false;
        if (brokenObj != null)
        {
            brokenObj.SetActive(false);
        }
        if (fixedObj != null)
        {
            fixedObj.SetActive(true);
        }
    }



    
    public void EnableGoo(bool fillImmediate = false)
    {
        if(fillImmediate)
            pipeLiquid.SetPipeFull();
        else
            pipeLiquid.FillPipe();
    }


    public void Save(){
        SaveSystem.Current.SetInt("mountainNumGems", numGems);
        SaveSystem.Current.SetBool("mountainGemMachineBroken", isBroken);
        
        SaveSystem.Current.SetInt("mountainGemMachinePhase", (int)phase);
    }

    public void Load(SaveProfile profile)
    {
        phase = (GemMachinePhase) profile.GetInt("mountainGemMachinePhase");

        numGems = profile.GetInt("mountainNumGems");
        if(numGems >= 2)
            isDone = true;
        isBroken = profile.GetBool("mountainGemMachineBroken", true);
        if(!isBroken)
            Fix();
        
        if(profile.GetBool("MountainGooFull"))
            EnableGoo(true);
        else if (profile.GetBool("MountainGooFilling"))
            EnableGoo();
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
