using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanGrid : SGrid
{
    public static OceanGrid instance;

    private static bool checkCompletion = false;

    public GameObject burriedGuyNPC;
    public KnotBox knotBox;

    public int totalCreditCount;
    private bool turnedInAnchor;
    private bool turnedInTreasureChest;
    private bool turnedInTreasureMap;
    private bool turnedInMushroom;
    private bool turnedInGoldenFish;
    private bool turnedInRock;
    private bool startedFinalQuest;

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

        }

        burriedGuyNPC.SetActive(false);

        AudioManager.PlayMusic("Ocean");
        UIEffects.FadeFromBlack();

    }
    
    private void OnEnable() 
    {
        if (checkCompletion) {
            SGrid.OnGridMove += SGrid.CheckCompletions;
        }

        SGridAnimator.OnSTileMoveEnd += CheckShipwreck;
        SGridAnimator.OnSTileMoveEnd += CheckVolcano;
    }

    private void OnDisable() 
    {
        if (checkCompletion) {
            SGrid.OnGridMove -= SGrid.CheckCompletions;
        }

        SGridAnimator.OnSTileMoveEnd -= CheckShipwreck;
        SGridAnimator.OnSTileMoveEnd -= CheckVolcano;
    }

    public override void SaveGrid() 
    {
        base.SaveGrid();
    }

    public override void LoadGrid()
    {
        base.LoadGrid();
    }


    public override void EnableStile(STile stile, bool shouldFlicker=true)
    {
        base.EnableStile(stile, shouldFlicker);
        
        stile.GetComponentInChildren<SpriteMask>().enabled = false; // on STile/SlideableArea
        
    }

    // === Temporary Tavernkeep Methods

    public void CheckTavernKeep()
    {
        // first talk
        ActivateSliderCollectible(3);

        // rest of rewards
        if (PlayerInventory.GetHasCollectedAnchor() && !turnedInAnchor)
        {
            turnedInAnchor = true;
            totalCreditCount += 2;
            // +1 because tile 4 is free in shop
            totalCreditCount += 1;
        }
        if (PlayerInventory.Contains("Treasure Chest") && !turnedInTreasureChest)
        {
            turnedInTreasureChest = true;
            totalCreditCount += 1;
        }
        if (PlayerInventory.Contains("Treasure Map") && !turnedInTreasureMap)
        {
            turnedInTreasureMap = true;
            totalCreditCount += 1;
        }
        if (PlayerInventory.Contains("Mushroom") && !turnedInMushroom)
        {
            turnedInMushroom = true;
            totalCreditCount += 1;
        }
        if (PlayerInventory.Contains("Golden Fish") && !turnedInGoldenFish)
        {
            turnedInGoldenFish = true;
            totalCreditCount += 1;
        }
        if (PlayerInventory.Contains("Rock") && !turnedInRock)
        {
            turnedInRock = true;
            totalCreditCount += 1;
        }

        // just activate in order for now
        for (int i = 4; i < Mathf.Min(4 + totalCreditCount, 10); i++)
        {
            ActivateSliderCollectible(i);
        }

        // check final quest on completing all others
        if (totalCreditCount == 7 + 1 && !startedFinalQuest) // todo: remove +1 later
        {
            startedFinalQuest = true;
            checkCompletion = true;
            SGrid.OnGridMove += SGrid.CheckCompletions;

            AudioManager.Play("Puzzle Complete");
        }
    }


    // === Ocean puzzle specific ===

    public void CheckShipwreck(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        // Debug.Log(IsShipwreckAdjacent());
        // Debug.Log(GetGridString());
        
        if (IsShipwreckAdjacent())
        {
            Collectible c = GetCollectible("Treasure Chest");
            
            if (!PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(true);
            }
        }
    }

    public bool IsShipwreckAdjacent()
    {
        return CheckGrid.contains(GetGridString(), "41");
    }

    public void CheckVolcano(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (IsVolcanoSet())
        {
            Collectible c = GetCollectible("Rock");

            if (!PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(true);
                AudioManager.Play("Puzzle Complete");
            }
        }
    }

    public bool IsVolcanoSet()
    {
        return CheckGrid.contains(GetGridString(),".4._895_.3.");
    }



    public void ActivateBurriedNPC()
    {
        burriedGuyNPC.SetActive(true);
    }

    private bool BuoyConditions()
    {
        if (!(GetStile(1).isTileActive && GetStile(3).isTileActive && GetStile(4).isTileActive && GetStile(8).isTileActive && GetStile(9).isTileActive))
        {
            return false;
        }

        if (!knotBox.isActiveAndEnabled)
        {
            return false;
        }

        return knotBox.CheckLines();
    }

    public void BuoyAllFound(Conditionals.Condition c)
    {
        if (!(GetStile(1).isTileActive && GetStile(3).isTileActive && GetStile(4).isTileActive && GetStile(8).isTileActive && GetStile(9).isTileActive))
        {
            c.SetSpec(false);
        }
        else
        {
            c.SetSpec(true);
        }
    }

    public void knotBoxEnabled(Conditionals.Condition c)
    {
        if (!knotBox.isActiveAndEnabled && (GetStile(1).isTileActive && GetStile(3).isTileActive && GetStile(4).isTileActive && GetStile(8).isTileActive && GetStile(9).isTileActive))
        {
            c.SetSpec(true);
        }
        else
        {
            c.SetSpec(false);
        }
    }

    public void BuoyCheck(Conditionals.Condition c)
    {
        c.SetSpec(BuoyConditions());
    }

    public void ToggleKnotBox()
    {
        knotBox.enabled = !knotBox.enabled;
    }

    public void IsCompleted(Conditionals.Condition c)
    {
        c.SetSpec(checkCompletion && CheckGrid.contains(GetGridString(), "412_..8_..."));
    }

}
