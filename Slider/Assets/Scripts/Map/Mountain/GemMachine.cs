using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemMachine : MonoBehaviour, ISavable
{
    private int numGems;
    private STile sTile;
    private bool isPowered;
    // private bool isDone;
    // private bool isBroken = true;
    public GameObject gemChecker;
    public Animator animator;

    public GameObject brokenObj;
    public GameObject fixedObj;
    public PipeLiquid pipeLiquid;
    
    public Minecart minecart;
    public Transform smokeTransform;
    
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

    public void BreakGemMachineCutscene()
    {
        BreakGemMachine();
    }

    public void BreakGemMachine() => BreakGemMachine(false);
    
    public void BreakGemMachine(bool fromSave = false)
    {
        gemMachineState = GemMachineState.BROKEN;
        brokenObj.SetActive(true);
        fixedObj.SetActive(false);
        gemChecker.SetActive(false);
        if(!fromSave)
        {   
            elevator.BreakElevator();
            AudioManager.Play("Slide Explosion");
            CameraShake.Shake(1f, 0.5f);
            for(int i = 0; i < 10; i++)
            {
                Vector3 random = Random.insideUnitCircle;
                ParticleManager.SpawnParticle(ParticleType.SmokePoof, smokeTransform.position + random);
            }
        }
    }

    public void FixGemMachine() => FixGemMachine(false);

    public void FixGemMachine(bool fromSave = false)
    {
        gemMachineState = GemMachineState.FIXED;
        brokenObj.SetActive(false);
        fixedObj.SetActive(true);
        gemChecker.SetActive(true);
    }

    public void AddGem(){
        if(!isPowered || gemMachineState == GemMachineState.BROKEN)
        {
            Debug.LogWarning("Gem machine took gem when it should not");
            return;
        }

        animator.Play("AbsorbGem");

        switch(gemMachineState)
        {
            case GemMachineState.INITIAL:
                numGems = 1;
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

    // public void Fix()
    // {
    //     isBroken = false;
    //     if (brokenObj != null)
    //     {
    //         brokenObj.SetActive(false);
    //     }
    //     if (fixedObj != null)
    //     {
    //         fixedObj.SetActive(true);
    //     }
    // }



    
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

    // public void CheckHasCrystals(Condition c){
    //     c.SetSpec(isDone);
    // }

    public void CheckIsPowered(Condition c){
        c.SetSpec(isPowered);
    }

    public void CheckIsBroken(Condition c)
    {
        c.SetSpec(gemMachineState == GemMachineState.BROKEN);
    }

    public void CheckIsFixed(Condition c){
        c.SetSpec(gemMachineState == GemMachineState.FIXED);
    }

    public void CheckHasFirstCrystal(Condition c){
        c.SetSpec(numGems == 1 && gemMachineState == GemMachineState.INITIAL);
    }

    public void CheckIntialCrystalCutscene(Condition c){
        c.SetSpec(isPowered && numGems == 1 && gemMachineState == GemMachineState.INITIAL);
    }
}
