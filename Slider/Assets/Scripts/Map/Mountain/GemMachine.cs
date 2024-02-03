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
    
    public Minecart minecart;
    
    public enum GemMachineState
    {
        INITIAL,
        BROKEN,
        FIXED,
        FULLY_GOOED
    }

    public GemMachineState gemMachineState = GemMachineState.INITIAL;

    public MinecartElevator elevator;

    private void Start() {
        sTile = GetComponentInParent<STile>();
    }

    // private void Update() {
    //     gemChecker.SetActive(isPowered && !isBroken);
    // }

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

    public void BreakGemMachine() => BreakGemMachine(false);
    
    public void BreakGemMachine(bool fromSave = false)
    {
        brokenObj.SetActive(true);
        fixedObj.SetActive(false);
        if(!fromSave)
            elevator.BreakElevator();
    }

    public void FixGemMachine() => FixGemMachine(false);

    public void FixGemMachine(bool fromSave = false)
    {
        brokenObj.SetActive(false);
        fixedObj.SetActive(true);
    }

    public void AddGem(){
        if(!isPowered || isBroken)
        {
            Debug.LogWarning("Gem machine took gem when it should not");
            return;
        }

        animator.Play("AbsorbGem");

        switch(gemMachineState)
        {
            case GemMachineState.INITIAL:
                print("take gem and blow up generator");
                break;
            case GemMachineState.BROKEN:
                print("this should not happen");
                break;
            case GemMachineState.FIXED:
                print("add gem");
                break;
            case GemMachineState.FULLY_GOOED:
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

    // private void IntialGemAbsorb()
    // {

    // }

    // private void AddGemToContainer()
    // {

    // }

    public void RemoveGem(){
        numGems--;
    }

    public void ResetGems(){
        if(gemMachineState != GemMachineState.FIXED) return;

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
        SaveSystem.Current.SetInt("mountainGemMachineNumGems", numGems);        
        SaveSystem.Current.SetInt("mountainGemMachinePhase", (int)gemMachineState);
    }

    public void Load(SaveProfile profile)
    {
        gemMachineState = (GemMachineState) profile.GetInt("mountainGemMachinePhase");

        // numGems = profile.GetInt("mountainNumGems");
        // if(numGems >= 2)
        //     isDone = true;
        // isBroken = profile.GetBool("mountainGemMachineBroken", true);
        // if(!isBroken)
        //     Fix();
        
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
