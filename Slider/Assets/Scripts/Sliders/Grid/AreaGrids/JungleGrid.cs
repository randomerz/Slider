using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleGrid : SGrid
{
    public ChadRace chadRace;

    public override void Init() {
        InitArea(Area.Jungle);
        base.Init();
    }
    

    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("Jungle");
        UIEffects.FadeFromBlack();
    }

    private void OnEnable() {
        SGrid.OnGridMove += CheckChad;
    }

    private void OnDisable() {
        SGrid.OnGridMove -= CheckChad;
    }

    public override void Save() 
    {
        base.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
    }

    public override void EnableStile(STile stile, bool shouldFlicker=true)
    {
        base.EnableStile(stile, shouldFlicker);
        CheckChad(this, null);
    }

    // === Jungle Puzzle Specific ===
    
    // Puzzle 5 - Chad Race
    public void CheckChad(object sender, SGrid.OnGridMoveArgs e) {
        if (Current.GetGrid() != null)
            chadRace.tilesAdjacent = CheckGrid.row(GetGridString(), "523") && GetStile(5).isTileActive && GetStile(2).isTileActive && GetStile(3).isTileActive;
    }
    
    public void OnRaceWon() {
        AudioManager.Play("Puzzle Complete");
    }

    public void SpawnChadRewards() {
        Collectible c = GetCollectible("Boots");
        if (!PlayerInventory.Contains(c))
        {
            c.gameObject.SetActive(true);
        }

        c = GetCollectible("Slider 6");
            
        if (!PlayerInventory.Contains(c))
        {
            c.gameObject.SetActive(true);
        }
    }

    public void PlayerCollectedRaceRewards(Condition c) {
        c.SetSpec(PlayerInventory.Contains(GetCollectible("Boots")));
    }
}
