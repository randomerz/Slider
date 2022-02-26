using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertGrid : SGrid
{
    public static DesertGrid instance;

    private int monkeShake = 0;
    private Vector2Int monkeyPrev = new Vector2Int(1, 1);

    private bool jackalBoned = false;

    private int die1 = 1;
    private int die2 = 1;

    private int numberAnimals = 0;
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

        SGridAnimator.OnSTileMove += CheckOasisOnMove;
    }

    private void OnDisable() {
        // if (checkCompletion) {
        //     SGrid.OnGridMove -= SGrid.CheckCompletions;
        // }

        SGridAnimator.OnSTileMove -= CheckOasisOnMove;
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

    //Puzzle 1: Oasis
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

            SGridAnimator.OnSTileMove += CheckMonkeyShakeOnMove;
            SGridAnimator.OnSTileMove += CheckJackalBoneOnMove;
            SGridAnimator.OnSTileMove += CheckMonkeyOasisOnMove;
            SGridAnimator.OnSTileMove += CheckJackalOasisOnMove;
            SGridAnimator.OnSTileMove -= CheckOasisOnMove;
        }
    }
    public bool CheckOasis()
    {
        // Debug.Log(CheckGrid.contains(GetGridString(), "2...1"));
        return CheckGrid.contains(GetGridString(), "2...1");
    }

    //Puzzle 2: Baboon tree shake
    public void CheckMonkeyShakeOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        //If the baboon is shaken awake
        if (CheckMonkeyShake())
        {
            Debug.Log("The Monkey has awoken!");
            //Baboon awakes
            //Slider actives?

            SGridAnimator.OnSTileMove -= CheckMonkeyShakeOnMove;
        }
    }
    public bool CheckMonkeyShake()
    {
        STile monkeyTile = SGrid.current.GetStile(3);
        if (Mathf.Abs(monkeyPrev.x - monkeyTile.x) == 2 || Mathf.Abs(monkeyPrev.y - monkeyTile.y) == 2)
        {
            //Shake the monkey. Logic for monkey stages of awake?
            monkeShake++;
            Debug.Log("The monkey got shook");
        }
        monkeyPrev = new Vector2Int(monkeyTile.x, monkeyTile.y);
        Debug.Log("Checked for monkey shake");

        if (monkeShake == 3)
        {
            return true;
        }
        return false;
    }

    public void CheckMonkeyOasisOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (monkeShake == 3 && CheckMonkeyNearOasis())
        {
            Debug.Log("Monkey joined sussy bday party");
            numberAnimals++;
            //Have monke join the bday crew
            SGridAnimator.OnSTileMove -= CheckMonkeyOasisOnMove;
        }
    }
    public bool CheckMonkeyNearOasis()
    {
        return CheckGrid.contains(GetGridString(), "(3|1)(1|3)") || CheckGrid.contains(GetGridString(), "(3|1)...(1|3)");
    }

    //Puzzle 3: Jackal Bone
    public void CheckJackalBoneOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckJackalBone())
        {
            Debug.Log("Jackal got dem boned");
            jackalBoned = true;
            SGridAnimator.OnSTileMove -= CheckJackalBoneOnMove;
        }
    }
    public bool CheckJackalBone()
    {
        return SGrid.current.GetStile(4).x == 0 && SGrid.current.GetStile(4).y == 2;
    }

    public void CheckJackalOasisOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (jackalBoned && CheckJackalNearOasis())
        {
            Debug.Log("Jackal be jacking up the hyped sus. Archaeologist bouta get some bones");
            //Jackal walk? Archaeologist stuff?
            //Spawn casino slider?
            numberAnimals++;
            SGridAnimator.OnSTileMove -= CheckJackalOasisOnMove;
        }
    }
    public bool CheckJackalNearOasis()
    {
        //Debug.Log("Checking Jackal and Oasis");
        //Debug.Log(GetGridString());
        return CheckGrid.contains(GetGridString(), "14") || CheckGrid.contains(GetGridString(), "1...4");
    }

    //Puzzle 4: Dice

    //Updates the dice while the casino isn't together
    public void CheckDiceOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        //If Casino is together
        if (CheckCasinoTogether())
        {
            if (die1 + die2 > 10)
            {
                Collectible c = GetCollectible("Slider 7");

                if (!PlayerInventory.Contains(c))
                {
                    c.gameObject.SetActive(true);
                }
                //activate chad dialogue
                SGridAnimator.OnSTileMove -= CheckDiceOnMove;
            }
        }
    }
    //Add this to the OnSTile Move when the dice game
    public void CheckDice(object sender, STile.STileMoveArgs e)
    {
    }
    public bool CheckCasinoTogether()
    {
        return CheckGrid.contains(GetGridString(), "56");
    }
}
