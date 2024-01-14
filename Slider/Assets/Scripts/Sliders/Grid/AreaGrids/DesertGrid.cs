using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DesertGrid : SGrid
{
    [Header("Desert")]
    public Item log; 
    public DistanceBasedAmbience campfireAmbience;
    public Animator crocodileAnimator;
    public Animator campfire;
    public DesertCasino DesertCasino;
    private bool campfireIsLit = false;
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
        AudioManager.PlayMusic("Desert");
        AudioManager.PlayMusic("Desert Casino", false);
        GiveTilesIfFromMagitech();
    }

    private void GiveTilesIfFromMagitech()
    {
        if(PlayerInventory.Contains("Slider 1", Area.MagiTech))
        {
            for (int i = 1; i <= 9; i++)
            Current.GetCollectible("Slider " + i)?.DoPickUp(true);
        }
    }
    
    private void OnEnable() {
        if (checkCompletion) {
            OnGridMove += UpdateButtonCompletions; 
            UIArtifact.OnButtonInteract += UpdateButtonCompletions;
        }
        if (campfireIsLit)
        {
            log.gameObject.SetActive(false);
            campfireAmbience.enabled = true;
            campfire.SetBool("isDying", false);
        }
    }

    private void OnDisable() {
        if (checkCompletion)
        {
            OnGridMove -= UpdateButtonCompletions;
            UIArtifact.OnButtonInteract -= SGrid.UpdateButtonCompletions;
        }
    }

    public override void EnableStile(STile stile, bool shouldFlicker = true)
    {
        base.EnableStile(stile, shouldFlicker);
        if(stile.islandId == 5 || stile.islandId == 6)
            DesertCasino.SyncSignAnimations();
    }

    public override STileTilemap GetWorldGridTilemaps()
    {
        int mirageIslandID;
        if(MirageSTileManager.GetInstance().IsPlayerOnMirage(out mirageIslandID))
        {
            return MirageSTileManager.GetInstance().GetMaterialTileMap(mirageIslandID);
        }
        return worldGridTilemaps;
    }

    /// <summary>
    /// Identical to <see cref="SGrid.GetGridString(bool)"/> except that this method considers
    /// mirage tiles. The ID used for a mirage tile id the Letter corresponding to the non-mirage tile 
    /// (e.g. the ID of the mirage tile of tile 1 is A.)
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
        SaveSystem.Current.SetBool("desertCamp", campfireIsLit);
        SaveSystem.Current.SetBool("desertCheckCompletion", checkCompletion);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        campfireIsLit = profile.GetBool("desertCamp");
        checkCompletion = profile.GetBool("desertCheckCompletion");
    }

    #region Oasis
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

    #region Gazelle
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
    public void ShufflePuzzle()
    {
        if (shuffleBuildUpCoroutine == null)
        {
            shuffleBuildUpCoroutine = StartCoroutine(ShuffleBuildUp());
        }
    }

    private IEnumerator ShuffleBuildUp()
    {
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
        UIArtifact.OnButtonInteract += UpdateButtonCompletions;
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
}