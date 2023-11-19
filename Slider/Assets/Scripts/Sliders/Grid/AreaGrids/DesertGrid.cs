using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DesertGrid : SGrid
{
    [Header("Desert")]
    public Item log; //Right now the animator for the campfire doesn't stay alive if scene transitions
    public DistanceBasedAmbience campfireAmbience;
    public Animator crocodileAnimator;
    public Animator campfire;
    public Item diceItem;
    public DiceGizmo dice1;
    public DiceGizmo dice2;
    public SpriteRenderer[] casinoCeilingSprites;
    public List<Animator> casinoSigns;
    [SerializeField] private ArtifactHousingButtonsManager artifactHousingButtonsManager;
    [SerializeField] private GameObject templeTrapBlockingRoom;
    [SerializeField] private GameObject templeTrapBlockingRoomCollider;
    [SerializeField] private Collider2D portalCollider; //Desert Portal
    [SerializeField] private MagiLaser portalLaser;

    private bool campfireIsLit = false;
    private bool portalEnabled = false;
    private bool portalLaserEnabled = false;
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
        
        if (SaveSystem.Current.GetBool("desertDiscoBallFell"))
        {
            RemoveDiceItem();
        }

        AudioManager.PlayMusic("Desert");
        AudioManager.PlayMusic("Desert Casino", false);
    }
    
    private void OnEnable() {
        if (checkCompletion) {
            OnGridMove += UpdateButtonCompletions; 
            UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
        }
        if (campfireIsLit)
        {
            log.gameObject.SetActive(false);
            campfireAmbience.enabled = true;
            campfire.SetBool("isDying", false);
        }

        if (SaveSystem.Current.GetBool("desertTempleActivatedTrap") &&
            !SaveSystem.Current.GetBool("desertTempleTrapCleared"))
        {
            ArtifactTabManager.AfterScrollRearrage += OnScrollRearrage;
        }

        portalCollider.enabled = portalEnabled;
        portalLaser.isEnabled = portalLaserEnabled;
    }

    private void OnDisable() {
        if (checkCompletion)
        {
            OnGridMove -= UpdateButtonCompletions;
            UIArtifact.OnButtonInteract -= SGrid.UpdateButtonCompletions;
        }

        if (SaveSystem.Current.GetBool("desertTempleActivatedTrap"))
        {
            ArtifactTabManager.AfterScrollRearrage -= OnScrollRearrage;
        }
    }

    private void Update() 
    {
        // For Casino music / sprites
        float distToCasino = GetDistanceToCasino();
        // AudioManager.SetMusicParameter("Desert", "DesertDistToCasino", distToCasino);
        AudioManager.SetGlobalParameter("DesertDistToCasino", distToCasino);

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

    /// <summary>
    /// Identical to <see cref="SGrid.GetGridString(bool)"/> except that this method considers
    /// mirage tiles. The ID used for a mirage tile id the ID of its equivalent non-mirage tile 
    /// (e.g. the ID of the mirage tile of tile 5 is 5.)
    /// </summary>
    /// <returns></returns>
    public static string GetGridString()
    {
        STile[,] grid = ((DesertGrid)Current).grid;

        Dictionary<Vector2Int, char> mirageTileIdsToPositions = MirageSTileManager.GetActiveMirageTileIdsByPosition();

        string s = "";
        for (int y = grid.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                if (grid[x, y].isTileActive)
                {
                    s += Converter.IntToChar(grid[x, y].islandId);
                }
                else
                {
                    Vector2Int tilePosition = new Vector2Int(x, y);
                    if (mirageTileIdsToPositions.ContainsKey(tilePosition))
                    {
                        s += mirageTileIdsToPositions[tilePosition];
                    } 
                    else
                    {
                        s += "#";
                    }
                }
            }
            if (y != 0)
            {
                s += "_";
            }
        }
        return s;
    }

    public override void Save() 
    {
        base.Save();

        //Bool Fun
        SaveSystem.Current.SetBool("desertCamp", campfireIsLit);
        SaveSystem.Current.SetBool("desertCheckCompletion", checkCompletion);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);

        campfireIsLit = profile.GetBool("desertCamp");
        checkCompletion = profile.GetBool("desertCheckCompletion");
        portalEnabled = profile.GetBool("magiTechDesertPortal");
        portalLaserEnabled = profile.GetBool("magiTechDesertLaser");

        if (SaveSystem.Current.GetBool("desertIsInTemple"))
        {
            SetIsInTemple(true);
        }
    }

    // === Desert puzzle specific ===
    #region Oasis
    //Puzzle 1: Oasis
    public void LightCampFire()
    {
        campfireIsLit = true;
        campfireAmbience.enabled = true;
        PlayerInventory.RemoveItem();
        log.gameObject.SetActive(false);
    }

    public void CheckCampfire(Condition c)
    {
        c.SetSpec(campfireIsLit);
    }
    #endregion

    #region Jackal
    //Puzzle 3: Jackal Bone
    public void CheckJackalNearOasis(Condition c) // no longer used
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
    public void RemoveDiceItem()
    {
        if (PlayerInventory.GetCurrentItem() == diceItem)
            PlayerInventory.RemoveItem();
        diceItem.gameObject.SetActive(false);
    }

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
        c.SetSpec(!SaveSystem.Current.GetBool("desertVIP") &&
            Player.GetPlayerAction().pickedItem != null && 
            Player.GetPlayerAction().pickedItem.itemName.Equals("Bottle"));
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
            c.SetSpec(cast.state == bottleState.clean || SaveSystem.Current.GetBool("desertVIP"));            
        }
        else
        {
            c.SetSpec(SaveSystem.Current.GetBool("desertVIP"));
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
        AudioManager.DampenMusic(this, 0.2f, 12);
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
        SaveSystem.Current.SetBool("desertShuffledGrid", true);

        yield return new WaitForSeconds(0.75f);

        CameraShake.Shake(2, 0.9f);
        shuffleBuildUpCoroutine = null;
    }

    public void GiveScrollAchievement() {
        AchievementManager.SetAchievementStat("collectedScroll", 1);
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
        if (!PlayerInventory.Contains("Slider 9", myArea) && gridString == "563_2#8_174" && placeTile9Coroutine == null)
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

    #region Scroll

    public void SetIsInTemple(bool isInTemple)
    {
        SaveSystem.Current.SetBool("desertIsInTemple", isInTemple);
        if (isInTemple)
        {
            SaveSystem.Current.SetBool("desertEnteredTemple", true);
        }

        artifactHousingButtonsManager.SetSpritesToHousing(isInTemple);
        Player._instance.SetTracker(!isInTemple);
        Player._instance.SetDontUpdateSTileUnderneath(isInTemple);
    }

    public void ActivateTrap()
    {
        SaveSystem.Current.SetBool("desertTempleTrapActivated", true);
        if (shuffleBuildUpCoroutine == null)
        {
            shuffleBuildUpCoroutine = StartCoroutine(ActivateTrapBuildUp());
        }
    }

    private IEnumerator ActivateTrapBuildUp()
    {
        templeTrapBlockingRoomCollider.SetActive(true);

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
        SGrid.Current.SetGrid(SGrid.GridStringToSetGridFormat("815493672"));
        templeTrapBlockingRoom.SetActive(true);
        SaveSystem.Current.SetBool("desertTempleActivatedTrap", true);

        ArtifactTabManager.AfterScrollRearrage += OnScrollRearrage;

        yield return new WaitForSeconds(0.75f);

        CameraShake.Shake(2, 0.9f);
        shuffleBuildUpCoroutine = null;
    }

    private void OnScrollRearrage(object sender, System.EventArgs e)
    {
        templeTrapBlockingRoom.SetActive(false);
        templeTrapBlockingRoomCollider.SetActive(false);
        SaveSystem.Current.SetBool("desertTempleTrapCleared", true);
        ArtifactTabManager.AfterScrollRearrage -= OnScrollRearrage;

        CheckFinalPlacements(UIArtifact.GetGridString());
        AchievementManager.SetAchievementStat("completedDesert", 1);
    }

    #endregion
}