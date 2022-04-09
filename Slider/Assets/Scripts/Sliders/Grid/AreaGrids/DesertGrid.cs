using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertGrid : SGrid
{
    public static DesertGrid instance;

    private bool crocoOasis = false;
    private bool crocoQuest = false;

    private int monkeShake = 0;
    private Vector2Int monkeyPrev = new Vector2Int(1, 1);
    private bool monkeyOasis = false;

    private bool jackalQuest = false;
    private bool jackalBoned = false;
    private bool jackalLeadToOasis = false;
    private bool jackalOasis = false;

    public DiceGizmo dice1;
    public DiceGizmo dice2;
    private bool challengedChad = false;
    private bool startDice = false;
    private bool diceWon = false;

    private bool VIPHelped = false;

    private bool GazelleQuest = false;
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
        if (dice1 == null && dice2 == null)
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
    public void SetCrocoOasis(bool b)
    {
        crocoOasis = b;
    }
    public void CheckArchTip(Conditionals.Condition c)
    {
        c.SetSpec(!crocoOasis);
    }
    public void CheckCrocoOasis(Conditionals.Condition c)
    {
        c.SetSpec(crocoOasis);
    }
    public void SetCrocoQuest(bool b)
    {
        crocoQuest = b;
    }
    public void CheckCrocoQuest(Conditionals.Condition c)
    {
        c.SetSpec(crocoQuest);
    }
    public void EnableMonkeyShake()
    {
        SGridAnimator.OnSTileMoveEnd += CheckMonkeyShakeOnMove;
    }

    //Puzzle 2: Baboon tree shake
    public void CheckMonkeyShake()
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
    }
    public void CheckMonkeyShakeOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        CheckMonkeyShake();
        if (monkeShake >= 3)
        {
            SGridAnimator.OnSTileMoveEnd -= CheckMonkeyShakeOnMove;
        }
    }
    //cond for monkey dialogue
    public void SetMonkeyOasis(bool b)
    {
        monkeyOasis = b;
    }
    public void IsAwake(Conditionals.Condition c)
    {
        c.SetSpec(monkeShake >= 3);
    }
    public void IsMonkeyNearOasis(Conditionals.Condition c)
    {
        c.SetSpec(CheckGrid.contains(GetGridString(), "(3|2)(2|3)") || CheckGrid.contains(GetGridString(), "(3|2)...(2|3)"));
    }
    public void IsMonkeyInOasis(Conditionals.Condition c)
    {
        c.SetSpec(monkeyOasis);
    }

    //Puzzle 3: Jackal Bone
    public void SetJackalQuest(bool b)
    {
        jackalQuest = b;
    }
    public void SetJackalBoned(bool b)
    {
        jackalBoned = b;
    }
    public void SetJackalLeadToOasis(bool b)
    {
        jackalLeadToOasis = b;
    }
    public void SetJackalOasis(bool b)
    {
        jackalOasis = b;
    }
    public void CheckJackalQuest(Conditionals.Condition c)
    {
        c.SetSpec(jackalQuest);
    }
    public void CheckJackalBoned(Conditionals.Condition c)
    {
        c.SetSpec(jackalBoned);
    }
    public void CheckJackalLeadToOasis(Conditionals.Condition c)
    {
        c.SetSpec(jackalLeadToOasis);
    }
    public void CheckJackalOasis(Conditionals.Condition c)
    {
        c.SetSpec(jackalOasis);
    }
    public void CheckJackalNearOasis(Conditionals.Condition c)
    {
       c.SetSpec(CheckGrid.contains(GetGridString(), "24") || CheckGrid.contains(GetGridString(), "2...4"));
    }
    public void CheckDinoNearArch(Conditionals.Condition c)
    {
        c.SetSpec(CheckGrid.contains(GetGridString(), "14") || CheckGrid.contains(GetGridString(), "1...4"));
    }

    //Puzzle 4: Dice. Should not start checking until after both tiles have been activated

    //Dconds for Chad dice game
    public void CheckChallenge(Conditionals.Condition c)
    {
        c.SetSpec(challengedChad);
    }
    public void CheckDice(Conditionals.Condition c)
    {
        c.SetSpec(dice1.isActiveAndEnabled && dice2.isActiveAndEnabled);
    }
    public void CheckRolledDice(Conditionals.Condition c)
    {
        c.SetSpec(startDice);
    }
    /*
    public void CheckPlayerChangedDice(Conditionals.Condition c)
    {
        c.SetSpec(startDice && dice1.value > 1 && dice2.value > 1);
    }
    */
    public void ChallengedChad()
    {
        challengedChad = true;
        Debug.Log("Chad challenged!");
    }
    //Activate stuff
    public void RollAndStartCountingDice()
    {
        startDice = true;
        Debug.Log("now the player can start rolling");
    }

    //Updates the dice while the casino isn't together
    public void CheckDiceValues(Conditionals.Condition c)
    {
        if (CheckCasinoTogether() && dice1.value + dice2.value > 10)
        {
            c.SetSpec(true);
            diceWon = true;
        }
        else
        {
            c.SetSpec(false);
        }
    }
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
        Item item = Player.GetPlayerAction().pickedItem;
        if (item != null && item.itemName.Equals("Bottle"))
        {
            Bottle cast = (Bottle)item;
            c.SetSpec(cast.state == bottleState.cactus);
        }
        else
        {
            c.SetSpec(false);
        }
    }
    public void IsDirtyWater(Conditionals.Condition c)
    {
        Item item = Player.GetPlayerAction().pickedItem;
        if (item != null && item.itemName.Equals("Bottle"))
        {
            Bottle cast = (Bottle)item;
            c.SetSpec(cast.state == bottleState.dirty);
        }
        else
        {
            c.SetSpec(false);
        }
    }
    public void IsCleanWater(Conditionals.Condition c)
    {
        Item item = Player.GetPlayerAction().pickedItem;
        if (item != null && item.itemName.Equals("Bottle"))
        {
            Bottle cast = (Bottle)item;
            c.SetSpec(cast.state == bottleState.clean);
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
    public void SetGazelleQuest(bool b)
    {
        GazelleQuest = b;
    }

    public void SetGazelleOasis(bool b)
    {
        GazelleOasis = b;
    }
    public void CheckGazelleQuest(Conditionals.Condition c)
    {
        c.SetSpec(GazelleQuest);
    }

    public void CheckGazelleNearOasis(Conditionals.Condition c)
    {
        c.SetSpec(CheckGrid.contains(GetGridString(), "26") || CheckGrid.contains(GetGridString(), "6...2"));
    }
    public void CheckGazelleOasis(Conditionals.Condition c)
    {
        c.SetSpec(GazelleOasis);
    }

    //Puzzle 7: 8puzzle
    public void ReAlignGrid()
    {
        //conduct series of STile swaps or something? Maybe set grid to something   
    }
} 