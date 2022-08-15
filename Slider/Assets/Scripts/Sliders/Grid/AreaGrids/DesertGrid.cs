using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DesertGrid : SGrid
{
    [Header("Desert")]
    public Item log; //Right now the animator for the campfire doesn't stay alive if scene transitions
    public NPCAnimatorController campfire;
    public DiceGizmo dice1;
    public DiceGizmo dice2;
    public SpriteRenderer[] casinoCeilingSprites;
    [SerializeField] private GameObject[] zlist; //From smallest to largest

    private int monkeShake = 0;
    private bool campfireIsLit = false;
    private bool checkCompletion = false;
    private bool checkMonkey = false;
    private Coroutine waitForZ; //Should be null if monkeShakes is 0

    public override void Init() {
        InitArea(Area.Desert);
        base.Init();
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
        // For Casino music / sprites
        float distToCasino = GetDistanceToCasino();
        AudioManager.SetMusicParameter("Desert", "DesertDistToCasino", distToCasino);

        // map [6, 8] => [0, 1]
        float alpha = Mathf.Clamp(Mathf.InverseLerp(6, 8, distToCasino), 0, 1);
        Color c = new Color(1, 1, 1, alpha);
        foreach (SpriteRenderer s in casinoCeilingSprites)
        {
            s.color = c;
        }
    }

    private float GetDistanceToCasino()
    {
        Vector3 pp = Player.GetPosition();
        STile s5 = GetStile(5);
        float s5x = s5.transform.position.x + Mathf.Clamp(pp.x - s5.transform.position.x, 0, 8.5f);
        float dist5 = s5.isTileActive ? (pp - new Vector3(s5x, s5.transform.position.y)).magnitude : 17; // center
        STile s6 = GetStile(6);
        float s6x = s6.transform.position.x + Mathf.Clamp(pp.x - s6.transform.position.x, -8.5f, 0);
        float dist6 = s6.isTileActive ? (pp - new Vector3(s6x, s6.transform.position.y)).magnitude : 17; // center
        return Mathf.Min(dist5, dist6);

    }

    public override void Save() 
    {
        base.Save();

        //Bool Fun
        SaveSystem.Current.SetBool("desertCamp", campfireIsLit);
        SaveSystem.Current.SetBool("desertCheckCompletion", checkCompletion);
        SaveSystem.Current.SetBool("desertCheckMonkey", checkMonkey);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);

        campfireIsLit = profile.GetBool("desertCamp");
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
    public void CheckCampfire(Condition c)
    {
        c.SetSpec(campfireIsLit);
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
        STile monkeyTile = Current.GetStile(3);
        if (monkeShake >= 3)
        {
            SGridAnimator.OnSTileMoveEnd -= CheckMonkeyShakeOnMove;
            if (waitForZ != null) StopCoroutine(MokeZTimer());
        }
        else if (monkeyTile.isTileActive && e.stile == monkeyTile)
        {
            Debug.Log(monkeShake);
            zlist[monkeShake].SetActive(false);
            monkeShake++;
            if (waitForZ == null) waitForZ = StartCoroutine(MokeZTimer()); //First shake starts countdown timer. waitForZ should be null if monkeShake is 0
        }
    }

    private IEnumerator MokeZTimer()
    {
        float time = 0f;
        int temp = 0;
        while (monkeShake > 0 && monkeShake < 3) //This is OMEGA SUS but 
        {
            time += Time.deltaTime;
            if (temp < monkeShake) //If monkeShake has changed, reset timer
            {
                time = 0f;
                temp = monkeShake;
            }
            if (time >= 2f)
            {
                monkeShake = monkeShake == 0 ? 0 : monkeShake - 1;
                temp = monkeShake;
                zlist[monkeShake].SetActive(true);
                time = 0f;
            }
            yield return null;
        }
        waitForZ = null;
    }

    public void IsAwake(Condition c)
    {
        c.SetSpec(monkeShake >= 3);
        checkMonkey = !(monkeShake >= 3);
    }
    public void IsMonkeyNearOasis(Condition c)
    {
        c.SetSpec(CheckGrid.contains(GetGridString(), "23") || CheckGrid.contains(GetGridString(), "(3|2)...(2|3)"));
    }
    #endregion

    #region Jackal
    //Puzzle 3: Jackal Bone
    public void CheckJackalNearOasis(Condition c)
    {
       c.SetSpec(CheckGrid.contains(GetGridString(), "24") || CheckGrid.contains(GetGridString(), "2...4"));
    }
    public void CheckDinoNearArch(Condition c)
    {
        c.SetSpec(CheckGrid.contains(GetGridString(), "14") || CheckGrid.contains(GetGridString(), "1...4"));
    }
    #endregion

    #region DicePuzzle
    //Puzzle 4: Dice. Should not start checking until after both tiles have been activated
    public void CheckRolledDice(Condition c)
    {
        c.SetSpec(dice1.isActiveAndEnabled && dice2.isActiveAndEnabled);
    }

    public void CheckDiceValues(Condition c)
    {
        if (CheckCasinoTogether() && dice1.value + dice2.value == 11) c.SetSpec(true);
        else if (SaveSystem.Current.GetBool("desertDice")) c.SetSpec(true);
        else c.SetSpec(false);
    }
    public bool CheckCasinoTogether()
    {
        return CheckGrid.contains(GetGridString(), "56");
    }

    #endregion

    #region VIPWater
    //Puzzle 5: Cactus Juice
    public void HasBottle(Condition c)
    {
        c.SetSpec(Player.GetPlayerAction().pickedItem != null && Player.GetPlayerAction().pickedItem.itemName.Equals("Bottle"));
    }
    public void IsCactusJuice(Condition c)
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
    public void IsDirtyWater(Condition c)
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
    public void IsCleanWater(Condition c)
    {
        Item item = Player.GetPlayerAction().pickedItem;
        if (item != null && item.itemName.Equals("Bottle"))
        {
            Bottle cast = (Bottle)item;
            c.SetSpec(cast.state == bottleState.clean);
            return;
        }
        if (SaveSystem.Current.GetBool("desertVIP"))
        {
            c.SetSpec(true);
        }
        else
        {
            c.SetSpec(false);
        }
    }
    #endregion

    #region Gazelle
    //Puzzle 6: Shady Gazelle
    public void CheckGazelleNearOasis(Condition c)
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