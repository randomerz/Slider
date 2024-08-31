using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWheel : MonoBehaviour, ISavable
{
    private const string HEATERS_ON_SAVE_STRING = "MountainGlobalHeatersOn";

    [SerializeField] private STile stile;
    [SerializeField] private Meltable cog1;
    [SerializeField] private Meltable cog2;
    private bool powered = false;
    [SerializeField] private ElectricalNode powerNode;
    public List<Animator> heaterAnimators;
    public bool heaterFixed = false;
    public bool heaterFull = false;
    public int lavaCount = 0;
    private bool firstPower = false;
    private bool firstLavaPower = false;
    public WaterWheelAnimator animator;
    public SpriteSwapper HeaterPipeSpriteSwapper;
    public Minecart mc;
    public Lava heaterLava;
    public PipeLiquid lavaPipe;
    public Animator lavaExtractorAnimator;
    public Meltable bigIce;

    public List<GameObject> heaterLavaGO;

    private void OnEnable() {
        Minecart.OnMinecartStop += () => CheckMinecartStop();
    }

    private void OnDisable() {
        Minecart.OnMinecartStop -= () => CheckMinecartStop();
    }


    private void Update() {
        UpdatePower();
    }

    public void UpdatePower() {
        bool shouldPower = stile.x == 0 && stile.y > 1 && cog1.IsNotFrozenOrBroken() && cog2.IsNotFrozenOrBroken() && !stile.IsMoving();
        powered = shouldPower;
        powerNode.StartSignal(shouldPower);
        if(!firstPower){
            firstPower = true;
            AudioManager.Play("Puzzle Complete");
        }
        if(powered && heaterFixed && !firstLavaPower){
            firstLavaPower = true;
            AudioManager.Play("Puzzle Complete");
        }
    }

    public void FillHeater()
    {
        SaveSystem.Current.SetBool(HEATERS_ON_SAVE_STRING, true);
        foreach(Animator a in heaterAnimators)
            a.Play("Fill");
    }

    public void OnFillHeaterEnd()
    {
        heaterFull = true;
        foreach(Animator a in heaterAnimators)
            a.Play("On");
        foreach(GameObject go in heaterLavaGO)
            go.SetActive(true);
        cog1.AddLava(heaterLava);
        cog2.AddLava(heaterLava);
        bigIce.AddLava(heaterLava);
        cog1.SetRefreezeOnTop(false);
        cog2.SetRefreezeOnTop(false);

        // This probably isn't the best way to do this...
        cog1.Save();
        cog2.Save();
        bigIce.Save();

        SaveSystem.Current.SetBool("MountainHeaterFull", true);
    }

    public void AddHeaterLava()
    {
        if(!heaterFixed) return;
        
        lavaCount++;
        mc.UpdateState(MinecartState.Empty);
        lavaExtractorAnimator.Play("Fill");
        if (lavaCount == 2)
        {
            AudioManager.Play("Puzzle Complete");
        }
        if (lavaCount > 2)
        {
            Debug.LogError($"lavaCount was more than 2! Calling OnFillHeaterEnd()");
            lavaCount = 2;
            OnFillHeaterEnd();
        }
    }

    public void OnEndAbsorbLava()
    {
        lavaExtractorAnimator.Play("Empty");
        if(lavaCount == 1)
        {
            lavaPipe.FillPipe(Vector2.zero, new Vector2(0, 0.5f), 3f);
        }
        if(lavaCount == 2){
            lavaPipe.FillPipe(new Vector2(0, 0.5f), Vector2.up, 3f);
        }
    }
    
    private void CheckMinecartStop()
    {
        if(heaterFixed && lavaCount == 1)
            ResetLavaOnMinecartStop();
    }

    public void ResetLavaOnMinecartStop()
    {
        lavaCount = 0;
        lavaExtractorAnimator.Play("Empty");
        lavaPipe.StopAllCoroutines();
        lavaPipe.SetPipeEmpty();
        AudioManager.Play("Artifact Error");
        SaveSystem.Current.SetBool("MountainLavaReset", true);
    }

    public void FixHeater() => FixHeater(false);

    public void FixHeater(bool fromSave=false) 
    {
        heaterFixed = true;
        HeaterPipeSpriteSwapper.TurnOn();

        if (!fromSave)
        {
            AudioManager.Play("Hat Click");
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, HeaterPipeSpriteSwapper.transform.position, HeaterPipeSpriteSwapper.transform);
        }
    }

    public bool IsDone() 
    {
        return lavaCount > 1 && powered;
    }

    #region specs

    public void IsInPosition(Condition c) {
        c.SetSpec(stile.x == 0 && stile.y > 1);
    }

    public void IsNotInPosition(Condition c) {
        c.SetSpec(stile.x == 1 || stile.y < 2);
    }
    
    public void IsWorking(Condition c) {
        c.SetSpec(stile.x == 0 && stile.y > 1 && cog1.IsNotFrozenOrBroken() && cog2.IsNotFrozenOrBroken());
    }

    public void BothGearsUnfrozen(Condition c) {
        c.SetSpec(cog1.IsNotFrozenOrBroken() && cog2.IsNotFrozenOrBroken());
    }

    public void ActiveLava(Condition c) {
        c.SetSpec(lavaCount > 0);
    }

    public void TwoLava(Condition c) {
        c.SetSpec(lavaCount >= 2);
    }

    public void IsDone(Condition c){
        c.SetSpec(lavaCount > 1 && powered);
    }

    public void IsHeaterFixed(Condition c){
        c.SetSpec(heaterFixed);
    }

    public void IsHeaterFull(Condition c){
        c.SetSpec(heaterFull);
    }
    

    #endregion

    public void Save()
    {
        SaveSystem.Current.SetBool("MountainWaterwheelLavaStage", heaterFixed);
        SaveSystem.Current.SetInt("MountainHeaterLavaCount", lavaCount);
        SaveSystem.Current.SetBool("MountainHeaterFirstPower", firstPower);
        SaveSystem.Current.SetBool("MountainHeaterFirstLavaPower", firstLavaPower);
    }

    public void Load(SaveProfile profile)
    {
        heaterFixed = profile.GetBool("MountainWaterwheelLavaStage", heaterFixed);
        lavaCount = profile.GetInt("MountainHeaterLavaCount", lavaCount);
        firstPower = profile.GetBool("MountainHeaterFirstPower", firstPower);
        firstLavaPower = profile.GetBool("MountainHeaterFirstLavaPower", firstPower);

        if (heaterFixed) FixHeater(fromSave: true);
        if (lavaCount == 1) lavaPipe.Fill(new(0, 0.5f));
        if (lavaCount == 2) 
        {
            lavaPipe.Fill(new(0, 1f));
            OnFillHeaterEnd();
        }
        else if (PlayerInventory.Contains("Slider 7", Area.Mountain))
        {
            Debug.LogError($"Player has Slider 7 Mountain, but the lava heater wasn't filled.");
            lavaCount = 2;
            OnFillHeaterEnd();
        }
    }

}
