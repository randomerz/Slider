using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DesertGrid : SGrid
{
    [Header("Desert")]
    public Item log; //Right now the animator for the campfire doesn't stay alive if scene transitions
    public Animator crocodileAnimator;
    public Animator campfire;
    public DiceGizmo dice1;
    public DiceGizmo dice2;
    public SpriteRenderer[] casinoCeilingSprites;
    public List<Animator> casinoSigns;
    [SerializeField] private GameObject[] zlist; //From smallest to largest
    [SerializeField] private Collider2D portalCollider; //Desert Portal
    [SerializeField] private MagiLaser portalLaser;

    private int monkeShake = 0;
    private bool campfireIsLit = false;
    private bool checkMonkey = false;
    private bool portalEnabled = false;
    private bool portalLaserEnabled = false;
    private Coroutine waitForZ; //Should be null if monkeShakes is 0
    private Coroutine shuffleBuildUpCoroutine;
    private Coroutine placeTile9Coroutine;

    private const string DESERT_PARTY_STARTED = "desertPartyStarted";
    private const string DESERT_PARTY_FINISHED = "desertPartyFinished";

    public override void Init() {
        InitArea(Area.Desert);
        base.Init();
    }

    protected override void Start()
    {
        base.Start();

        if (dice1 == null && dice2 == null) Debug.LogWarning("Die have not been set!");
        if (log == null) Debug.LogWarning("Log has not been set!");

        AudioManager.PlayMusic("Desert");
        AudioManager.PlayMusic("Desert Casino", false);
    }
    
    private void OnEnable() {
        if (checkCompletion) {
            OnGridMove -= UpdateButtonCompletions; 
            UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
        }
        if (checkMonkey)
        {
            SGridAnimator.OnSTileMoveEnd += CheckMonkeyShakeOnMove;
        }
        if (campfireIsLit)
        {
            log.gameObject.SetActive(false);
            campfire.SetBool("isDying", false);
        }
        Debug.Log("PortalEnabled: " + portalEnabled);
        Debug.Log("PortalLaser: " + portalLaserEnabled);
        portalCollider.enabled = portalEnabled;
        portalLaser.isEnabled = portalLaserEnabled;
    }

    private void OnDisable() {
        if (checkCompletion)
        {
            OnGridMove -= UpdateButtonCompletions;
            UIArtifact.OnButtonInteract -= SGrid.UpdateButtonCompletions;
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

    public override void EnableStile(STile stile, bool shouldFlicker = true)
    {
        base.EnableStile(stile, shouldFlicker);
        if(stile.islandId == 5 || stile.islandId == 6)
            foreach (Animator a in casinoSigns)
                a.Play("Idle", -1, 0);
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
        portalEnabled = profile.GetBool("magiTechDesertPortal");
        portalLaserEnabled = profile.GetBool("magiTechDesertLaser");
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
        if (monkeyTile.isTileActive && e.stile == monkeyTile)
        {
            zlist[monkeShake].SetActive(false);
            monkeShake++;

            if (monkeShake >= 3)
            {
                // puzzle complete
                AudioManager.PlayWithPitch("Baboon Screech", 1.5f); // TODO: make this affected by distance

                SGridAnimator.OnSTileMoveEnd -= CheckMonkeyShakeOnMove;
                checkMonkey = false;
                if (waitForZ != null) StopCoroutine(MokeZTimer());
                return;
            }
            else
            {
                AudioManager.Play("Baboon Screech"); // TODO: make this affected by distance

                if (waitForZ != null) StopCoroutine(MokeZTimer());
                waitForZ = StartCoroutine(MokeZTimer()); //First shake starts countdown timer. waitForZ should be null if monkeShake is 0
            }
        }
    }

    private IEnumerator MokeZTimer()
    {
        float time = 0f;
        while (monkeShake > 0 && monkeShake < 3) //This is OMEGA SUS but 
        {
            time += Time.deltaTime;
            if (time >= 3f)
            {
                monkeShake = monkeShake == 0 ? 0 : monkeShake - 1;
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

    #region Party
    public void StartParty()
    {
        if (SaveSystem.Current.GetBool(DESERT_PARTY_STARTED) || SaveSystem.Current.GetBool(DESERT_PARTY_FINISHED))
            return;

        SaveSystem.Current.SetBool(DESERT_PARTY_STARTED, true);

        StartCoroutine(PartyCutscene());
    }

    private IEnumerator PartyCutscene()
    {
        crocodileAnimator.SetTrigger("grab");

        CameraShake.ShakeIncrease(3, 0.4f);
        AudioManager.DampenMusic(0.2f, 12);
        AudioManager.Play("Crocodile Grab Sequence");

        yield return new WaitForSeconds(11);

        SaveSystem.Current.SetBool(DESERT_PARTY_FINISHED, true);
    }
    #endregion

    #region 8puzzle
    //Puzzle 7: 8puzzle
    public void ShufflePuzzle()
    {
        if (shuffleBuildUpCoroutine == null)
        {
            shuffleBuildUpCoroutine = StartCoroutine(ShuffleBuildUp());
        }
    }

    private IEnumerator ShuffleBuildUp()
    {
        //AudioManager.Play("Puzzle Complete");

        //yield return new WaitForSeconds(0.5f);

        CameraShake.Shake(0.25f, 0.25f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(0.25f, 0.25f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(0.75f, 0.5f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(1.5f, 2.5f);
        AudioManager.PlayWithVolume("Slide Explosion", 0.2f);
        AudioManager.Play("TFT Bell");

        yield return new WaitForSeconds(0.25f);

        UIEffects.FlashWhite();
        DoShuffle();

        yield return new WaitForSeconds(0.75f);

        CameraShake.Shake(2, 0.9f);
    }

    private void DoShuffle()
    {
        if (GetNumTilesCollected() != 8)
        {
            Debug.LogError("Tried to shuffle desert when not 8 tiles were collected! Detected " + GetNumTilesCollected() + " tiles.");
            return;
        }

        DesertArtifactRandomizer.ShuffleGrid();

        gridAnimator.ChangeMovementDuration(0.5f);

        checkCompletion = true;
        SaveSystem.Current.SetBool("desertCompletion", checkCompletion);

        OnGridMove += UpdateButtonCompletions;
        UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
    }
    
    protected override void UpdateButtonCompletionsHelper()
    {
        base.UpdateButtonCompletionsHelper();

        CheckFinalPlacements(UIArtifact.GetGridString());
    }

    private void CheckFinalPlacements(string gridString)
    {
        if (!PlayerInventory.Contains("Slider 9", myArea) && gridString == "567_2#3_184" && placeTile9Coroutine == null)
        {
            AudioManager.Play("Puzzle Complete");

            // Disable artifact movement
            UIArtifact.DisableMovement(false); // TODO: make sure this works with scrap of the scroll

            placeTile9Coroutine = StartCoroutine(PlaceTile9());
        }
    }

    private IEnumerator PlaceTile9()
    {
        yield return new WaitUntil(() => UIArtifact._instance.MoveQueueEmpty());

        GivePlayerTheCollectible("Slider 9");

        // we don't have access to the Collectible.StartCutscene() pick up, so were doing this dumb thing instead
        StartCoroutine(CheckCompletionsAfterDelay(1.2f));

        UIArtifactWorldMap.SetAreaStatus(myArea, ArtifactWorldMapArea.AreaStatus.color);
        UIArtifactMenus._instance.OpenArtifactAndShow(2, true);
        
        placeTile9Coroutine = null;
    }

    #endregion
}