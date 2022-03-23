using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleGrid : SGrid
{
    public static JungleGrid instance;

    public ChadRace chadRace;

    private static bool checkCompletion = false;

    private new void Awake() {
        myArea = Area.Jungle;

        foreach (Collectible c in collectibles) 
        {
            c.SetArea(myArea);
        }

        base.Awake();

        instance = this;
    }
    

    void Start()
    {
        foreach (Collectible c in collectibles) 
        {
            if (PlayerInventory.Contains(c)) 
            {
                c.gameObject.SetActive(false);
            }
        }
        
        AudioManager.PlayMusic("Connection");
        UIEffects.FadeFromBlack();
    }

    private void OnEnable() {
        SGrid.OnGridMove += CheckChad;
    }

    private void OnDisable() {
        SGrid.OnGridMove -= CheckChad;
    }

    public override void SaveGrid() 
    {
        base.SaveGrid();
    }

    public override void LoadGrid()
    {
        base.LoadGrid();
    }

    // === Jungle Puzzle Specific ===
    
    // Puzzle 5 - Chad Race
    public void CheckChad(object sender, SGrid.OnGridMoveArgs e) {
        chadRace.tilesAdjacent = CheckGrid.row(GetGridString(), "523") && GetStile(5).isTileActive && GetStile(2).isTileActive && GetStile(3).isTileActive;
    }
    // Invoked by the Chad race script
    public void onPlayerWinRace() {
        Collectible c = GetCollectible("Boots");
            
            if (!PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(true);
            }
    }
}
