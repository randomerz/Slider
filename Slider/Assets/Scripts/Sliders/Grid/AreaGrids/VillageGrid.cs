using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageGrid : SGrid
{
    public static VillageGrid instance;

    private static bool checkCompletion = false;

    public Collectible[] sliderCollectibles;

    private new void Awake() {
        instance = this;

        base.Awake();
    }
    
    private new void OnEnable() {
        if (checkCompletion) {
            SGrid.OnGridMove += SGrid.CheckCompletions;
        }
        
    }

    private new void OnDisable() {
        if (checkCompletion) {
            SGrid.OnGridMove -= SGrid.CheckCompletions;
        }
    }

    void Start()
    {
        for (int x = 3; x < sliderCollectibles.Length; x++)
        {
            sliderCollectibles[x].gameObject.SetActive(false);
        }
    }

    public void ActivateSliderCollectible(int sliderId) {
        sliderCollectibles[sliderId - 1].gameObject.SetActive(true);

        if (sliderId == 9)
        {
            sliderCollectibles[sliderId - 1].transform.position = Player.GetPosition();
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
}
