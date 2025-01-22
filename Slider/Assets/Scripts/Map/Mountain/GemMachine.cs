using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemMachine : MonoBehaviour, ISavable
{
    public int numGems;
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
        // If you quit during the animations happening
        if (gemMachineState == GemMachineState.FULLY_GOOED)
        {
            numGems = 3;
        }
        if (numGems >= 3)
        {
            pipeLiquid.SetPipeFull();
        }
    }

    private void OnEnable() {
        SGridAnimator.OnSTileMoveStart += CheckMove;
        Minecart.OnMinecartStop += () => CheckMinecartStop();
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveStart -= CheckMove;
        Minecart.OnMinecartStop -= () => CheckMinecartStop();
    }

    public void Save()
    {
        ResetGemLoop(false, true); // You have to do the loop 
        SaveSystem.Current.SetInt("mountainGemMachineNumGems", numGems);        
        SaveSystem.Current.SetInt("mountainGemMachinePhase", (int) gemMachineState);
    }

    public void Load(SaveProfile profile)
    {
        gemMachineState = (GemMachineState) profile.GetInt("mountainGemMachinePhase");
        numGems = SaveSystem.Current.GetInt("mountainGemMachineNumGems");        
        switch(gemMachineState)
        {
            case GemMachineState.BROKEN:
                BreakGemMachine(true);
                break;
            case GemMachineState.FIXED:
                FixGemMachine(true);
                break;
        }
    }

    private void CheckMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile != sTile) return;
        switch (gemMachineState)
        {
            case GemMachineState.INITIAL:
                ResetFirstGem();
                break;
            case GemMachineState.FIXED:
                ResetGemLoop(false, false);
                break;
        }
    }

    private void CheckMinecartStop()
    {
        if(gemMachineState == GemMachineState.FIXED)
        {
            ResetGemLoop(true, false);
        }
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
        if (!fromSave)
        {   
            elevator.BreakElevator();
            AudioManager.Play("Slide Explosion");
            CameraShake.Shake(1f, 0.5f);
            for (int i = 0; i < 10; i++)
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
        SaveSystem.Current.SetBool("MountainFixedGemMachine", true);


        if (!fromSave)
        {
            AudioManager.Play("Hat Click");
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, fixedObj.transform.position, fixedObj.transform);
        }

        brokenObj.SetActive(false);
        fixedObj.SetActive(true);
        gemChecker.SetActive(true);
        if(numGems < 1) numGems = 1;
    }

    public void AddGem() {
        if (gemMachineState == GemMachineState.BROKEN)
        {
            return;
        }
        if (!isPowered)
        {
            SaveSystem.Current.SetBool("MountainTriedCrystalNoPower", true);
            return;
        }

        switch (gemMachineState)
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
        minecart.UpdateState(MinecartState.Empty, false);
        animator.Play("part1");
        AudioManager.Play("Gem Sucker", transform);
    }

    private void RepairedGemAbsorb()
    {
        if (!isPowered)
            return; 
        numGems++;
        animator.Play("AbsorbGem");
        minecart.UpdateState(MinecartState.Empty, false);
        AudioManager.Play("Gem Sucker", transform);
        if (numGems >= 3)
        {
            FinishCrystalLoopPuzzle();
        }
    }

    private void FinishCrystalLoopPuzzle()
    {
        AudioManager.Play("Puzzle Complete");
        gemMachineState = GemMachineState.FULLY_GOOED;
    }

    public void ResetFirstGem()
    {
        if (numGems != 1) return;
        numGems = 0;
        animator.Play("Empty");
        SaveSystem.Current.SetBool("MountainFirstGemReset", true);
        AudioManager.Play("Artifact Error");
    }

    public void ResetGemLoop(bool minecart, bool fromSave)
    {
        if (numGems != 2) return;
        numGems = 1;
        animator.Play("Empty");
        pipeLiquid.StopAllCoroutines();
        pipeLiquid.SetPipeEmpty();
        if (fromSave) return;
        if (minecart)
        {
            SaveSystem.Current.SetBool("MountainGemResetMinecart", true);
            SaveSystem.Current.SetBool("MountainGemResetMove", false);

        }
        else
        {
            SaveSystem.Current.SetBool("MountainGemResetMove", true);
            SaveSystem.Current.SetBool("MountainGemResetMinecart", false);
        }

        AudioManager.Play("Artifact Error");
    }

    public void OnEndAbsorb() 
    {
        animator.Play("Empty");
        if (numGems == 2)
        {
            pipeLiquid.FillPipe(Vector2.zero, new Vector2(0, 0.5f), 3f);
        }
        if (numGems == 3)
        {
            pipeLiquid.FillPipe(new Vector2(0, 0.5f), Vector2.up, 3f);
        }
    }

    public void SetIsPowered(bool value){
        isPowered = value;
    }

    public void CheckIsPowered(Condition c) => c.SetSpec(isPowered);
    public void CheckIsBroken(Condition c) => c.SetSpec(gemMachineState == GemMachineState.BROKEN);
    public void CheckIsFixed(Condition c) => c.SetSpec(gemMachineState == GemMachineState.FIXED);
    public void CheckIsBrokenAndPowered(Condition c) => c.SetSpec(gemMachineState == GemMachineState.BROKEN && isPowered);
    public void CheckIsFixedAndPowered(Condition c) => c.SetSpec(gemMachineState == GemMachineState.FIXED && isPowered);
    public void CheckHasFirstCrystal(Condition c) => c.SetSpec(numGems == 1 && gemMachineState == GemMachineState.INITIAL);
    public void CheckHasFirstGooCrystal(Condition c) => c.SetSpec(numGems == 2 && gemMachineState == GemMachineState.FIXED);
    public void CheckHasSecondGooCrystal(Condition c) => c.SetSpec(numGems >= 3 && (gemMachineState == GemMachineState.FIXED || gemMachineState == GemMachineState.FULLY_GOOED));
    public void CheckIntialCrystalCutscene(Condition c) => c.SetSpec(isPowered && numGems == 1 && gemMachineState == GemMachineState.INITIAL);
}
