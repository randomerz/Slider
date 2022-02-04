using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertGrid : SGrid
{
    public static DesertGrid instance;

    private static bool checkCompletion = false;

    public Collectible[] collectibles;

    private int monkeyShakeMoves = 0;
    private bool[] animalList = { false, false, false };

    public void ActivateSliderCollectible(int sliderId)
    { // temporary?
        collectibles[sliderId - 1].gameObject.SetActive(true);

        if (sliderId == 9)
        {
            collectibles[sliderId - 1].transform.position = Player.GetPosition();
            UIManager.closeUI = true;
        }

        AudioManager.Play("Puzzle Complete");
    }
    private new void Awake() {
        myArea = Area.Desert;

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
    private void OnEnable()
    {
        if (checkCompletion)
        {
            SGrid.OnGridMove += SGrid.CheckCompletions;
        }

        SGridAnimator.OnSTileMove += CheckOasisOnMove;
        SGridAnimator.OnSTileMove += CheckMonkeyShakeOnMove;
        SGridAnimator.OnSTileMove += CheckMonkeyOasisOnMove;
        SGridAnimator.OnSTileMove += CheckJackalBoneOnMove;
        SGridAnimator.OnSTileMove += CheckJackalOasisOnMove;
    }

    private void OnDisable()
    {
        if (checkCompletion)
        {
            SGrid.OnGridMove -= SGrid.CheckCompletions;
        }

        SGridAnimator.OnSTileMove -= CheckOasisOnMove;
        SGridAnimator.OnSTileMove -= CheckMonkeyShakeOnMove;
        SGridAnimator.OnSTileMove -= CheckMonkeyOasisOnMove;
        SGridAnimator.OnSTileMove -= CheckJackalBoneOnMove;
        SGridAnimator.OnSTileMove -= CheckJackalOasisOnMove;
    }

    // === Desert puzzle specific ===

    //Puzzle 2 - Oasis
    private void CheckOasisOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckOasis())
        {
            ActivateSliderCollectible(3);
            ActivateSliderCollectible(4);
        }
    }
    private bool CheckOasis()
    {
        return CheckGrid.contains(GetGridString(), "2..1");
    }

    //Puzzle 3 - Monke Shake
    //Figure out how to tell if monkey tile (3) slid 2 tiles over
    private void CheckMonkeyShakeOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (monkeyShakeMoves == 3)
        {
            //Shake the Monkey down command and have them follow
        }
    }
        //Checks if the monke tile is adjacent to oasis
    private bool CheckMonkeyNearOasis()
    {
        return CheckGrid.contains(GetGridString(), "(3|1)(1|3)") || CheckGrid.contains(GetGridString(), "(3|1)..(1|3)");
    }
    private void CheckMonkeyOasisOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckMonkeyNearOasis())
        {
            animalList[0] = true;
            //Have monke join the bday crew
        }
    }

    //Puzzle 4 - Jackal Bone
    private bool CheckJackalBone()
    {
        //return Player has given bone
        return false; //remove this
    }
    private void CheckJackalBoneOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckJackalBone())
        {
            //Have jackal give player bone he's nomming on
            //Have jackal follow the player
        }
    }
    private bool CheckJackalNearOasis()
    {
        return CheckGrid.contains(GetGridString(), "(4|1)(1|4)") || CheckGrid.contains(GetGridString(), "(4|1)..(1|4)");
    }
    private void CheckJackalOasisOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckJackalNearOasis())
        {
            animalList[1] = true;
            //Have jackal join the bday crew
        }
    }

    //Save and Load
    public override void SaveGrid()
    {
        base.SaveGrid();
    }

    public override void LoadGrid()
    {
        base.LoadGrid();
    }
}
