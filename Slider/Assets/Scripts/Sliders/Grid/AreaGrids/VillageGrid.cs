using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageGrid : SGrid
{
    public static VillageGrid instance;

    private static bool checkCompletion = false;

    public Collectible[] sliderCollectibles;

    private new void Awake() {
        myArea = Area.Village;

        base.Awake();

        instance = this;
    }
    
    private void OnEnable() {
        if (checkCompletion) {
            SGrid.OnGridMove += SGrid.CheckCompletions;
        }
        
    }

    private void OnDisable() {
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

    // public override void SetSGrid(SGrid other)
    // {
    //     Debug.Log("A");
    //     base.SetSGrid(other);

    //     // TODO: Update collectibles and quests and progress and stuff
    //     VillageGrid vg = (VillageGrid)other;
    //     for (int i = 0; i < sliderCollectibles.Length; i++) {
    //         if (vg.sliderCollectibles[i] != null) {
    //             sliderCollectibles[i].gameObject.SetActive(vg.sliderCollectibles[i].gameObject.activeSelf);
    //         }
    //     }
    // }

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
