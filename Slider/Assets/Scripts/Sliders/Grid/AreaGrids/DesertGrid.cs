using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertGrid : SGrid
{
    public static DesertGrid instance;

    private bool campfireIsLit = false;
    public Item log; //Right now the animator for the campfire doesn't stay alive if scene transitions
    public NPCAnimatorController campfire;

    private bool crocoOasis = false;

    private int monkeShake = 0;
    private bool monkeyOasis = false;

    private bool jackalBoned = false;
    private bool jackalOasis = false;
    private bool archDino = false;

    public DiceGizmo dice1;
    public DiceGizmo dice2;
    private bool diceWon = false;

    private bool VIPHelped = false;

    private bool gazelleOasis = false;

    private bool checkCompletion = false;
    private bool checkMonkey = false;

    public override void Init() {
        myArea = Area.Desert;

        foreach (Collectible c in collectibles)
        {
            c.SetArea(myArea);
        }

        base.Init();

        instance = this;
    }

    protected override void Start()
    {
        base.Start();

        if (dice1 == null && dice2 == null) Debug.LogWarning("Die have not been set!");
        if (log == null) Debug.LogWarning("Log has not been set!");

        if (!campfireIsLit)
        {
            log.gameObject.SetActive(true);
            campfire.SetBoolToTrue("isDying");
        }
        AudioManager.PlayMusic("Desert");
        AudioManager.PlayMusic("Desert Casino", false);
        UIEffects.FadeFromBlack();
    }
    
    private void OnEnable() {
        if (checkCompletion) {
            UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
            SGridAnimator.OnSTileMoveEnd += CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletions
        }
        if (checkMonkey)
        {
            SGridAnimator.OnSTileMoveEnd += CheckMonkeyShakeOnMove;
        }
    }

    private void OnDisable() {
        if (checkCompletion)
        {
            OnGridMove -= UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract -= SGrid.UpdateButtonCompletions;
            SGridAnimator.OnSTileMoveEnd -= CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletions
        }
        if (checkMonkey)
        {
            SGridAnimator.OnSTileMoveEnd -= CheckMonkeyShakeOnMove;
        }
    }

    private void Update() 
    {
        // For Casino music
        
        STile s5 = SGrid.current.GetStile(5);
        float dist1 = s5.isTileActive ? (Player.GetPosition() - s5.transform.position).magnitude : 17; // center
        float dist2 = s5.isTileActive ? (Player.GetPosition() - (s5.transform.position + Vector3.right * 8.5f)).magnitude : 17; // right
        STile s6 = SGrid.current.GetStile(6);
        float dist3 = s6.isTileActive ? (Player.GetPosition() - s6.transform.position).magnitude : 17; // center
        float dist4 = s6.isTileActive ? (Player.GetPosition() - (s6.transform.position + Vector3.left * 8.5f)).magnitude : 17; // left
        AudioManager.SetMusicParameter("Desert", "DesertDistToCasino", Mathf.Min(dist1, dist2, dist3, dist4));
    }

    public override void Save() 
    {
        base.Save();

        //Bool Fun
        SaveSystem.Current.SetBool("desertCamp", campfireIsLit);
        SaveSystem.Current.SetBool("desertCrocoOasis", crocoOasis);
        SaveSystem.Current.SetBool("desertMonkeyOasis", monkeyOasis);
        SaveSystem.Current.SetBool("desertJackalBoned", jackalBoned);
        SaveSystem.Current.SetBool("desertJackalOasis", jackalOasis);
        SaveSystem.Current.SetBool("desertVIP", VIPHelped);
        SaveSystem.Current.SetBool("desertGazelleOasis", gazelleOasis);
        SaveSystem.Current.SetBool("desertCheckCompletion", checkCompletion);
        SaveSystem.Current.SetBool("desertCheckMonkey", checkMonkey);
        SaveSystem.Current.SetBool("desertArchDino", archDino);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);

        campfireIsLit = profile.GetBool("desertCamp");
        crocoOasis = profile.GetBool("desertCrocoOasis");
        monkeyOasis = profile.GetBool("desertMonkeyOasis");
        jackalBoned = profile.GetBool("desertJackalBoned");
        jackalOasis = profile.GetBool("desertJackalOasis");
        archDino = profile.GetBool("desertArchDino");
        VIPHelped = profile.GetBool("desertVIP");
        gazelleOasis = profile.GetBool("desertGazelleOasis");
        checkCompletion = profile.GetBool("desertCheckCompletion");
        checkMonkey = profile.GetBool("desertCheckMonkey");
    }

    // === Desert puzzle specific ===
    #region Oasis
    //Puzzle 1: Oasis
    public void LightCampFire()
    {
        campfireIsLit = true;
        PlayerInventory.RemoveItem();
        log.gameObject.SetActive(false);
    }
    public void CheckCampfire(Conditionals.Condition c)
    {
        c.SetSpec(campfireIsLit);
    }
    public void SetCrocoOasis(bool b)
    {
        crocoOasis = b;
    }
    public void EnableMonkeyShake()
    {
        SGridAnimator.OnSTileMoveEnd += CheckMonkeyShakeOnMove;
        checkMonkey = true;
    }
    #endregion

    #region Monkey
    //Puzzle 2: Baboon tree shake
    public void CheckMonkeyShakeOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        STile monkeyTile = SGrid.current.GetStile(3);
        if (e.stile == SGrid.current.GetStile(3))
        {
            if (Mathf.Abs(e.prevPos.x - monkeyTile.x) == 2 || Mathf.Abs(e.prevPos.y - monkeyTile.y) == 2)
            {
                //Shake the monkey. Logic for monkey stages of awake?
                monkeShake++;
                Debug.Log("The monkey got shook");
            }
            else
            {
                monkeShake = 0;
                Debug.Log("Monkey shakes reset!");
            }
        }
        if (monkeShake >= 3)
        {
            SGridAnimator.OnSTileMoveEnd -= CheckMonkeyShakeOnMove;
        }
    }
    public void SetMonkeyOasis(bool b)
    {
        monkeyOasis = b;
    }
    public void IsAwake(Conditionals.Condition c)
    {
        c.SetSpec(monkeShake >= 3);
        checkMonkey = !(monkeShake >= 3);
    }
    public void IsMonkeyNearOasis(Conditionals.Condition c)
    {
        c.SetSpec(CheckGrid.contains(GetGridString(), "(3|2)(2|3)") || CheckGrid.contains(GetGridString(), "(3|2)...(2|3)"));
    }
    public void IsFirstShake(Conditionals.Condition c)
    {
        c.SetSpec(monkeShake >= 1);
    }
    public void IsSecondShake(Conditionals.Condition c)
    {
        c.SetSpec(monkeShake >= 2);
    }
    #endregion

    #region Jackal
    //Puzzle 3: Jackal Bone
    public void SetJackalBoned(bool b)
    {
        jackalBoned = b;
    }
    public void SetJackalOasis(bool b)
    {
        jackalOasis = b;
    }
    public void SetArchDino(bool b)
    {
        archDino = b;
    }
    public void CheckJackalNearOasis(Conditionals.Condition c)
    {
       c.SetSpec(CheckGrid.contains(GetGridString(), "24") || CheckGrid.contains(GetGridString(), "2...4"));
    }
    public void CheckDinoNearArch(Conditionals.Condition c)
    {
        c.SetSpec(CheckGrid.contains(GetGridString(), "14") || CheckGrid.contains(GetGridString(), "1...4"));
    }
    #endregion

    #region DicePuzzle
    //Puzzle 4: Dice. Should not start checking until after both tiles have been activated
    public void CheckRolledDice(Conditionals.Condition c)
    {
        c.SetSpec(dice1.isActiveAndEnabled && dice2.isActiveAndEnabled);
    }

    public void CheckDiceValues(Conditionals.Condition c)
    {
        if (CheckCasinoTogether() && dice1.value + dice2.value == 11)
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

    #endregion

    #region VIPWater
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
    public void SpawnShades()
    {
        Collectible c = GetCollectible("Sunglasses");
        VIPHelped = true;
        if (!PlayerInventory.Contains(c))
        {
            c.gameObject.SetActive(true);
        }
    }

    #endregion

    #region Gazelle
    //Puzzle 6: Shady Gazelle
    public void SetGazelleOasis(bool b)
    {
        gazelleOasis = b;
    }
    public void CheckGazelleNearOasis(Conditionals.Condition c)
    {
        c.SetSpec(CheckGrid.contains(GetGridString(), "26") || CheckGrid.contains(GetGridString(), "6...2"));
    }

    #endregion

    #region 8puzzle
    //Puzzle 7: 8puzzle
    public void ShufflePuzzle()
    {
        DesertArtifactRandomizer.ShuffleGrid();

        // fading stuff
        UIEffects.FlashWhite();
        CameraShake.Shake(1.5f, 1.0f);

        checkCompletion = true;
        OnGridMove += UpdateButtonCompletions; // this is probably not needed
        UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
        SGridAnimator.OnSTileMoveEnd += CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletions
    }
    private void CheckFinalPlacementsOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (!PlayerInventory.Contains("Slider 9", Area.Desert) && (GetGridString() == "567_2#3_184"))
        {
            GivePlayerTheCollectible("Slider 9");

            // Disable queues
            UIArtifact.ClearQueues();

            // we don't have access to the Collectible.StartCutscene() pick up, so were doing this dumb thing instead
            StartCoroutine(CheckCompletionsAfterDelay(1.1f));

            AudioManager.Play("Puzzle Complete");
            UIArtifactWorldMap.SetAreaStatus(Area.Desert, ArtifactWorldMapArea.AreaStatus.color);
        }
    }
    #endregion
}