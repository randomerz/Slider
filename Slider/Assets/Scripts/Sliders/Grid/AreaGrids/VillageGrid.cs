using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageGrid : SGrid
{
    public static VillageGrid instance;

    private static bool checkCompletion = false;

    public Collectible[] collectibles;


    // bad
    public static bool wasQRCompleted = false;
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


    // === Village puzzle specific ===


    // Puzzle 5 - R&J 
    // Checks if Romeo (tile 1) and Juliette (tile 5) are next to each other using Regex
    public bool CheckLovers()
    {
        return CheckGrid.row(GetGridString(), "15.") || CheckGrid.row(GetGridString(), ".15");
    }

    // Puzzle 6 - QR Code
    // This method is added to SGridAnimator.OnSTileMove above in OnEnable
    // Don't forget to remove it in OnDisable, or bad things will happen when unloaded!
    private void CheckQRCodeOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckQRCode())
        {
            ActivateSliderCollectible(7);
        }
    }

    private bool CheckQRCode()
    {
        if (wasQRCompleted)
        {
            return false;
        }

        //Debug.Log("Checking qr code");
        wasQRCompleted = CheckGrid.subgrid(GetGridString(), "3162");

        return wasQRCompleted;
    }


    // Puzzle 7 - River
    // Checks if the river tiles are in order with Regex (see puzzle doc for the proper order)
    public bool CheckRiver()
    {
        return CheckGrid.contains(GetGridString(), "624_..7_...");
    }


    // Puzzle 8 - 8puzzle
    public void ShufflePuzzle() {
        int[,] shuffledPuzzle = new int[3, 3] { { 7, 0, 1 },
                                                { 6, 4, 8 },
                                                { 5, 3, 2 } };
        SetGrid(shuffledPuzzle);

        // fading stuff
        UIEffects.FlashWhite();

        checkCompletion = true;
        OnGridMove += CheckCompletions; // SGrid.OnGridMove += SGrid.CheckCompletions
    }


    private void CheckFinalPlacementsOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckFinalPlacements())
        {
            ActivateSliderCollectible(9);
            CheckCompletions(this, null); // lazy
        }
    }

    public static bool CheckFinalPlacements()
    {
        return !PlayerInventory.Contains("Slider 9", Area.Village) && (GetGridString() == "624_8#7_153");
    }
}
