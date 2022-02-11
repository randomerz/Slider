using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanGrid : SGrid
{
    public static OceanGrid instance;

    private static bool checkCompletion = false;

    // public Collectible[] collectibles;

    private new void Awake() {
        myArea = Area.Ocean;

        foreach (Collectible c in collectibles) // maybe don't have this
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

            if (c.GetName() == "Slider 5")
            {
                c.gameObject.SetActive(false);
            }
        }

        AudioManager.PlayMusic("Connection");
        UIEffects.FadeFromBlack();

    }
    
    private void OnEnable() {
        // if (checkCompletion) {
        //     SGrid.OnGridMove += SGrid.CheckCompletions;
        // }

        SGridAnimator.OnSTileMove += CheckShipwreck;
    }

    private void OnDisable() {
        // if (checkCompletion) {
        //     SGrid.OnGridMove -= SGrid.CheckCompletions;
        // }

        SGridAnimator.OnSTileMove -= CheckShipwreck;
    }

    public override void SaveGrid() 
    {
        base.SaveGrid();
    }

    public override void LoadGrid()
    {
        base.LoadGrid();
    }


    // === Ocean puzzle specific ===

    public void CheckShipwreck(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (IsShipwreckAdjacent())
        {
            Collectible c = GetCollectible("Slider 5");
            
            if (!PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(true);
            }
        }
    }

    public bool IsShipwreckAdjacent()
    {
        return CheckGrid.row(GetGridString(), "41");
    }
}
