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

    // Called by NPC sara
    public void BreakGemMachineCutscene()
    {
        StartCoroutine(DoBreakGemMachineCutscene());
    }

    private IEnumerator DoBreakGemMachineCutscene()
    {
        yield return new WaitForSeconds(1);

        AudioManager.PlayWithVolume("Slide Explosion", 0.2f);
        CameraShake.Shake(0.6f, 0.2f);

        yield return new WaitForSeconds(0.5f);

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

        if (!fromSave)
        {
            AudioManager.Play("Hat Click");
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, fixedObj.transform.position, fixedObj.transform);
        }

        brokenObj.SetActive(false);
        fixedObj.SetActive(true);
        gemChecker.SetActive(true);
    }

    public void AddGem(){
        if(gemMachineState == GemMachineState.BROKEN)
        {
            return;
        }

        switch(gemMachineState)
        {
            case GemMachineState.INITIAL:
                IntialGemAbsorb();
                break;
            case GemMachineState.FIXED:
            case GemMachineState.FULLY_GOOED:
                RepairedGemAbsorb();
                break;
        }
    }

    private void IntialGemAbsorb()
    {
        if(numGems == 1) return;
        numGems = 1;
        minecart.UpdateState(MinecartState.Empty);
        animator.Play("part1");
        //TODO: Play absorb crystal sound
    }

    private void RepairedGemAbsorb()
    {
        if(!isPowered)
            return;
        numGems++;
        animator.Play("AbsorbGem");
        minecart.UpdateState(MinecartState.Empty);
        //TODO: Play absorb crystal sound
    }

    public void ResetGems(){
        if(gemMachineState != GemMachineState.FIXED) return;

        numGems = 1;
    }

    public void OnEndAbsorb(){
        animator.Play("Empty");
    }

    public void SetIsPowered(bool value){
        isPowered = value;
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
        switch(gemMachineState)
        {
            case GemMachineState.BROKEN:
                BreakGemMachine(true);
                break;
            case GemMachineState.FIXED:
                FixGemMachine(true);
                break;
        }
        if (profile.GetBool("MountainGooFull"))
            EnableGoo(true);
        else if (profile.GetBool("MountainGooFilling"))
            EnableGoo();
    }

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

    public void CheckIsBrokenAndPowered(Condition c){
        c.SetSpec(gemMachineState == GemMachineState.BROKEN && isPowered);
    }

    public void CheckIsFixedAndPowered(Condition c){
        c.SetSpec(gemMachineState == GemMachineState.FIXED && isPowered);
    }

    public void CheckHasFirstCrystal(Condition c){
        c.SetSpec(numGems == 1 && gemMachineState == GemMachineState.INITIAL);
    }

    public void CheckHasFirstGooCrystal(Condition c){
        c.SetSpec(numGems == 2 && gemMachineState == GemMachineState.FIXED);
    }

    public void CheckHasSecondGooCrystal(Condition c){
        c.SetSpec(numGems >= 3 && gemMachineState == GemMachineState.FIXED);
    }

    public void CheckIntialCrystalCutscene(Condition c){
        c.SetSpec(isPowered && numGems == 1 && gemMachineState == GemMachineState.INITIAL);
    }
}
