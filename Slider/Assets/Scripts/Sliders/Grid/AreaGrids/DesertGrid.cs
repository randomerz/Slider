using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertGrid : SGrid
{
    public static DesertGrid instance;

    private int monkeShake = 0;
    private Vector2Int monkeyPrev = new Vector2Int(1, 1);
    private bool monkeyOasis = false;

    private bool jackalBoned = false;
    private bool jackalOasis = false;

    [SerializeField] private DiceGizmo die1;
    [SerializeField] private DiceGizmo die2;
    private bool startDice = false;
    private bool diceWon = false;

    private bool VIPHelped = false;
    private bool GazelleOasis = false;

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

        AudioManager.PlayMusic("Desert");
        UIEffects.FadeFromBlack();
    }
    
    private void OnEnable() {
        // if (checkCompletion) {
        //     SGrid.OnGridMove += SGrid.CheckCompletions;
        // }

        //SGridAnimator.OnSTileMoveEnd += CheckOasisOnMove;
    }

    private void OnDisable() {
        // if (checkCompletion) {
        //     SGrid.OnGridMove -= SGrid.CheckCompletions;
        // }

        //SGridAnimator.OnSTileMoveEnd -= CheckOasisOnMove;
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
    /*
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
    */

    //Puzzle 2: Baboon tree shake
    public void CheckMonkeyShakeOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        //If the baboon is shaken awake
        if (SGrid.current.GetActiveTiles().Contains(SGrid.current.GetStile(3)) && CheckMonkeyShake())
        {
            Debug.Log("The Monkey has awoken!");

            //Baboon awakes
            monkeyOasis = true;

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
        // have an else?
        monkeyPrev = new Vector2Int(monkeyTile.x, monkeyTile.y);
        Debug.Log("Checked for monkey shake");

        if (monkeShake == 3)
        {
            return true;
        }
        return false;
    }

    //cond for monkey dialogue
    public void IsAwake(Conditionals.Condition c)
    {
        c.SetSpec(monkeShake >= 3);
    }

    public void CheckMonkeyOasisOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (monkeShake == 3 && CheckMonkeyNearOasis())
        {
            Debug.Log("Monkey joined sussy bday party");
            //Have monke join the bday crew
            monkeyOasis = true;
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
    public void IsMonkeyNearOasis(Conditionals.Condition c)
    {
        c.SetSpec(CheckMonkeyNearOasis());
    }
    public void IsMonkeyInOasis(Conditionals.Condition c)
    {
        c.SetSpec(monkeyOasis);
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

    //Chen: Method to "Spawn the dice" and roll the 5's for Chad at first
    public void EnableDice()
    {
        //Enable both dice?
        die1.gameObject.SetActive(true);
        die2.gameObject.SetActive(true);
        Debug.Log("Dice got rolled by chad");
    }

    //Conditon for Chad to enable the rolling of dice
    public void CheckDice(Conditionals.Condition c)
    {
        c.SetSpec(die1.isActiveAndEnabled && die2.isActiveAndEnabled);
    }
    public void RollAndStartCountingDice()
    {
        if (startDice)
        {
            ////WWIP
        }
        die1.value = 1;
        die2.value = 1;
        SGridAnimator.OnSTileMoveEnd += CheckDiceOnMove;
        Debug.Log("now the player can start rolling");
    }

    //Updates the dice while the casino isn't together
    public void CheckDiceOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        //If Casino is together. As a nod to speedrunners, the player does NOT have to be in the casino for the slider to spawn
        if (CheckCasinoTogether())
        {
            if (die1.value + die2.value > 10)
            {
                Collectible c = GetCollectible("Slider 7");
                //Debug.Log("Enabled slider 7");
                diceWon = true;

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
    public void IsDiceWon(Conditionals.Condition c)
    {
        c.SetSpec(diceWon);
    }

    //Puzzle 5: Cactus Juice
    public void HasBottle(Conditionals.Condition c)
    {
        c.SetSpec(Player.GetPlayerAction().pickedItem != null && Player.GetPlayerAction().pickedItem.itemName.Equals("Bottle"));
    }
    //Chen: Dcond tests for bottle type
    public void IsCactusJuice(Conditionals.Condition c)
    {
        Bottle item = (Bottle)Player.GetPlayerAction().pickedItem;
        if (item != null && item.itemName.Equals("Bottle"))
        {
            c.SetSpec(item.state == bottleState.cactus);
        }
        else
        {
            c.SetSpec(false);
        }
    }
    public void IsDirtyWater(Conditionals.Condition c)
    {
        Bottle item = (Bottle) Player.GetPlayerAction().pickedItem;
        if (item != null && item.itemName.Equals("Bottle"))
        {
            c.SetSpec(item.state == bottleState.dirty);
        }
        else
        {
            c.SetSpec(false);
        }
    }
    public void IsCleanWater(Conditionals.Condition c)
    {
        Bottle item = (Bottle)Player.GetPlayerAction().pickedItem;
        if (item != null && item.itemName.Equals("Bottle"))
        {
            c.SetSpec(item.state == bottleState.clean);
        }
        else
        {
            c.SetSpec(false);
        }
    }
    public void CheckVIPHelped(Conditionals.Condition c)
    {
        c.SetSpec(VIPHelped);
    }
    public void SpawnShades()
    {
        Collectible c = GetCollectible("Sunglasses");
        Debug.Log("Shades creep up to your neck");
        VIPHelped = true;
        if (!PlayerInventory.Contains(c))
        {
            c.gameObject.SetActive(true);
        }
    }
    

    //Puzzle 6: Shady Gazelle
    public void EnableGazelleOasisCheck()
    {
        SGridAnimator.OnSTileMoveEnd += CheckGazelleNearOasisOnMove;
    }

    public void CheckGazelleNearOasisOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (CheckGazelleNearOasis())
        {
            Collectible c = GetCollectible("Slider 8");
            Debug.Log("Gazelle got dem munchlaxxed");
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

    public void ReAlignGrid()
    {
        //conduct series of STile swaps or something? Maybe set grid to something   
    }
} 