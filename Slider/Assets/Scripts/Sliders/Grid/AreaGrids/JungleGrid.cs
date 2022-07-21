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
        if (GetNumTilesCollected() == 3)
        {
            string s = GetGridString(true);
            int location2 = s.IndexOf("2");
            int x2 = 2 - (location2 % 3);
            int y2 = 2 - (location2 / 3);
            STile two = grid[x2, y2];

            bool doubleSwap = false;
            if (!CheckGrid.contains(s, "23"))
            {
                print("swapping");

                //we also have to check for occupancy AHHHHH
                if (x2 == 2 && stile.x == 0)
                {
                    doubleSwap = true;
                } else if (stile.x == 0)
                {
                    STile other = grid[x2 + 1, y2];
                    if (other.isTileActive)
                    {
                        doubleSwap = true;
                    }
                    else
                    {
                        int x = other.x;
                        int y = other.y;
                        other.SetGridPosition(stile.x, stile.y);
                        stile.SetGridPosition(x, y);
                        grid[other.x, other.y] = grid[x, y];
                        grid[x, y] = stile;

                        UIArtifact.SetButtonPos(3, x, y);
                        UIArtifact.SetButtonPos(other.islandId, other.x, other.y);
                    }
                } else
                {
                    STile other = grid[stile.x - 1, stile.y];
                    if (other.isTileActive)
                    {
                        doubleSwap = true;
                    }
                    else
                    {
                        int x = other.x;
                        int y = other.y;

                        other.SetGridPosition(x2, y2);
                        two.SetGridPosition(x, y);
                        grid[x, y] = two;
                        grid[x2, y2] = other;

                        UIArtifact.SetButtonPos(2, x, y);
                        UIArtifact.SetButtonPos(other.islandId, other.x, other.y);
                    }
                }
            } else if (CheckGrid.contains(s, "23") && x2 == 2 && stile.x == 0)
            {
                doubleSwap = true;
            }

            if (doubleSwap)
            {

            }
        }
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
