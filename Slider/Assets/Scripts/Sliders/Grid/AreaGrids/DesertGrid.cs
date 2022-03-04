using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertGrid : SGrid
{
    public static DesertGrid instance;

    private int monkeShake = 0;
    private Vector2Int monkeyPrev = new Vector2Int(1, 1);

    private bool jackalBoned = false;

    [SerializeField] private DiceGizmo die1;
    [SerializeField] private DiceGizmo die2;

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
        if (die1 == null && die2 == null)
        {
            Debug.LogWarning("Die have not been set!");
        }
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

            SGridAnimator.OnSTileMoveEnd += CheckMonkeyShakeOnMove;
            SGridAnimator.OnSTileMoveEnd += CheckMonkeyOasisOnMove;
            SGridAnimator.OnSTileMoveEnd += CheckJackalBoneOnMove;
            SGridAnimator.OnSTileMoveEnd += CheckJackalOasisOnMove;
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
        if (SGrid.current.GetActiveTiles().Contains(SGrid.current.GetStile(3)) && CheckMonkeyShake())
        {
            Debug.Log("The Monkey has awoken!");
            //Baboon awakes
            SGridAnimator.OnSTileMoveEnd -= CheckMonkeyShakeOnMove;
            SGridAnimator.OnSTileMoveEnd += CheckMonkeyOasisOnMove;
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
            //Have monke join the bday crew
            Collectible c = GetCollectible("Slider 5");

            if (!PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(true);
            }
            SGridAnimator.OnSTileMoveEnd -= CheckMonkeyOasisOnMove;
        }
    }
    public bool CheckMonkeyNearOasis()
    {
        return CheckGrid.contains(GetGridString(), "(3|2)(2|3)") || CheckGrid.contains(GetGridString(), "(3|2)...(2|3)");
    }

    //Puzzle 3: Jackal Bone
    public void CheckJackalBoneOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (SGrid.current.GetActiveTiles().Contains(SGrid.current.GetStile(4)) && CheckJackalBone())
        {
            Debug.Log("Jackal got dem boned");
            jackalBoned = true;
            SGridAnimator.OnSTileMoveEnd -= CheckJackalBoneOnMove;
            SGridAnimator.OnSTileMoveEnd += CheckJackalOasisOnMove;
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
            Collectible c = GetCollectible("Slider 6");

            if (!PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(true);
            }

            SGridAnimator.OnSTileMoveEnd += CheckDiceOnMove;
            SGridAnimator.OnSTileMoveEnd += CheckShadesOnMove;
            SGridAnimator.OnSTileMoveEnd -= CheckJackalOasisOnMove;
        }
    }
    public bool CheckJackalNearOasis()
    {
        //Debug.Log("Checking Jackal and Oasis");
        //Debug.Log(GetGridString());
        return CheckGrid.contains(GetGridString(), "24") || CheckGrid.contains(GetGridString(), "2...4");
    }

    //Puzzle 4: Dice. Should not start checking until after both tiles have been activated

    //Updates the dice while the casino isn't together
    public void CheckDiceOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        //If Casino is together. As a nod to speedrunners, the player does NOT have to be in the casino for the slider to spawn
        if (CheckCasinoTogether())
        {
            if (die1.value + die2.value > 10)
            {
                Collectible c = GetCollectible("Slider 7");
                Debug.Log("Enabled slider 7");

                if (!PlayerInventory.Contains(c))
                {
                    c.gameObject.SetActive(true);
                }
                //activate chad dialogue
                SGridAnimator.OnSTileMoveEnd -= CheckDiceOnMove;
            }
        }
    }
    //Add this to the OnSTile Move when the dice game
    public bool CheckCasinoTogether()
    {
        return CheckGrid.contains(GetGridString(), "56");
    }

    //Puzzle 6: Shady Gazelle
    public void CheckShadesOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckShades())
        {
            //Gazelle Dialogue
            Debug.Log("Got the shades");
            SGridAnimator.OnSTileMoveEnd -= CheckShadesOnMove;
            SGridAnimator.OnSTileMoveEnd += CheckGazelleNearOasisOnMove;
        }
    } 
    public bool CheckShades()
    {
        return PlayerInventory.Contains("Sunglasses");
    }

    public void CheckGazelleNearOasisOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckGazelleNearOasis())
        {
            Collectible c = GetCollectible("Slider 8");

            if (!PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(true);
            }

            SGridAnimator.OnSTileMoveEnd -= CheckGazelleNearOasisOnMove;
        }
    }
    public bool CheckGazelleNearOasis()
    {
        Debug.Log("Checking Gazelle");
        return (CheckGrid.contains(GetGridString(), "26") || CheckGrid.contains(GetGridString(), "6...2"));
    }
} 