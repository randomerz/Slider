using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertGrid : SGrid
{
    public static DesertGrid instance;

    private static bool checkCompletion = false;

    // public Collectible[] collectibles;

    private new void Awake() {
        myArea = Area.Desert;

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

        AudioManager.PlayMusic("Connection");
        UIEffects.FadeFromBlack();
    }
    
    private void OnEnable() {
        // if (checkCompletion) {
        //     SGrid.OnGridMove += SGrid.CheckCompletions;
        // }

        SGridAnimator.OnSTileMoveEnd += CheckOasisOnMove;
    }

    private void OnDisable() {
        // if (checkCompletion) {
        //     SGrid.OnGridMove -= SGrid.CheckCompletions;
        // }

        SGridAnimator.OnSTileMoveEnd -= CheckOasisOnMove;
    }

    public override void SaveGrid() 
    {
        base.SaveGrid();
    }

    public override void LoadGrid()
    {
        base.LoadGrid();
    }


    // === Desert puzzle specific ===

    public void CheckOasisOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckOasis())
        {
            Collectible c = GetCollectible("Slider 3");
            Collectible d = GetCollectible("Slider 4");

            if (!PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(true);
            }
            if (!PlayerInventory.Contains(d))
            {
                d.gameObject.SetActive(true);
            }
        }
    }
    public bool CheckOasis()
    {
        // Debug.Log(CheckGrid.contains(GetGridString(), "2...1"));
        return CheckGrid.contains(GetGridString(), "2...1");
    }

}
