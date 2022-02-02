using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageGrid : SGrid
{
    public static VillageGrid instance;

    private static bool checkCompletion = false;

    public Collectible[] collectibles;


    // bad
    public static bool hasBeenDug = false;
    public static bool firstTimeFezziwigCheck = false;

    private new void Awake() {
        myArea = Area.Village;

        foreach (Collectible c in collectibles) 
        {
            c.SetArea(myArea);
        }

        base.Awake();

        instance = this;
    }
    
    private void OnEnable() {
        if (checkCompletion) {
            SGrid.OnGridMove += SGrid.CheckCompletions;
        }
        
        SGridAnimator.OnSTileMove += CheckQRCodeOnMove;
        SGridAnimator.OnSTileMove += CheckFinalPlacementsOnMove;
    }

    private void OnDisable() {
        if (checkCompletion) {
            SGrid.OnGridMove -= SGrid.CheckCompletions;
        }
        
        SGridAnimator.OnSTileMove -= CheckQRCodeOnMove;
        SGridAnimator.OnSTileMove -= CheckFinalPlacementsOnMove;
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

    public override void SaveGrid() 
    {
        base.SaveGrid();

        // GameManager.saveSystem.SaveSGridData(Area.Village, this);
        // GameManager.saveSystem.SaveMissions(new Dictionary<string, bool>());
    }

    public override void LoadGrid()
    {
        base.LoadGrid();
    }

    public void ActivateSliderCollectible(int sliderId) { // temporary?
        collectibles[sliderId - 1].gameObject.SetActive(true);

        if (sliderId == 9)
        {
            collectibles[sliderId - 1].transform.position = Player.GetPosition();
            UIManager.closeUI = true;
        }

        AudioManager.Play("Puzzle Complete");
    }


    // puzzle specific stuff
    public void ShufflePuzzle() {
        int[,] shuffledPuzzle = new int[3, 3] { { 7, 0, 1 },
                                                { 6, 4, 8 },
                                                { 5, 3, 2 } };
        SetGrid(shuffledPuzzle);

        // fading stuff

        checkCompletion = true;
        SGrid.OnGridMove += SGrid.CheckCompletions;
    }

    private void CheckQRCodeOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckQRCode())
        {
            // ItemManager.ActivateNextItem();
            VillageGrid.instance.ActivateSliderCollectible(7);
            //Debug.Log("Activated QR work already");
        }
    }

    public static bool CheckQRCode()
    {
        if (hasBeenDug)
        {
            return false;
        }
        //Debug.Log("Checking qr code");
        hasBeenDug = CheckGrid.subgrid(SGrid.GetGridString(), "3162");

        return hasBeenDug;
    }

    public bool CheckLovers()
    {
        return CheckGrid.row(SGrid.GetGridString(), "15.") || CheckGrid.row(SGrid.GetGridString(), ".15");
    }


    private void CheckFinalPlacementsOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckFinalPlacements())
        {
            // ItemManager.ActivateNextItem();
            VillageGrid.instance.ActivateSliderCollectible(9);
        }
    }

    public static bool CheckFinalPlacements()
    {
        return !PlayerInventory.Contains("Slider 9", Area.Village) && (SGrid.GetGridString() == "624_8#7_153");
    }
}
