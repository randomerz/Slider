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
                        SwapTiles(stile, other);
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
                        SwapTiles(two, other);
                    }
                }
            } else if (CheckGrid.contains(s, "23") && x2 == 2 && stile.x == 0)
            {
                doubleSwap = true;
            }

            if (doubleSwap)
            {
                List<STile> tiles = new List<STile>();
                //options for tile 2
                for (int i = 0; i < 2; i ++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        tiles.Add(grid[i, j]);
                    }
                }

                foreach (STile tile in tiles)
                {
                    if (tile.isTileActive)
                    {
                        continue;
                    }
                    if (grid[tile.x + 1, tile.y].isTileActive)
                    {
                        continue;
                    }

                    STile other2 = tile;
                    STile other3 = grid[tile.x + 1, tile.y];

                    SwapTiles(other2, two);
                    SwapTiles(other3, stile);
                    break;
                }
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
