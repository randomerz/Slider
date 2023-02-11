using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageGrid : SGrid
{
    public GameObject caveDoorEntrance;
    public GameObject caveDoorRocks;
    public GameObject particleSpawner;

    private bool fishOn;

    [SerializeField] private RuinsSymbols ruinsSymbols;
    [SerializeField] private GameObject ruinsFragment; // for finishing caves before village
    [SerializeField] private Transform slider8FloorTransform; // for finishing caves before village
    private Coroutine shuffleBuildUpCoroutine;
    private Coroutine placeTile9Coroutine;
    

    public CameraDolly introCameraDolly;
    private const string INTRO_CUTSCENE_SAVE_STRING = "villageIntroCutscene";

    public override void Init()
    {
        InitArea(Area.Village);
        base.Init();

        if (fishOn)
        {
            particleSpawner.GetComponent<ParticleSpawner>().SetFishOn();
        }
    }

    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("Village");
        
        CheckHole();
    }

    private void OnEnable()
    {
        if (checkCompletion)
        {
            SGrid.OnGridMove += SGrid.UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
        }
    }

    private void OnDisable()
    {
        if (checkCompletion)
        {
            SGrid.OnGridMove -= SGrid.UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract -= SGrid.UpdateButtonCompletions;
        }
    }

    public override void Save()
    {
        base.Save();

        SaveSystem.Current.SetBool("villageCompletion", checkCompletion);
        SaveSystem.Current.SetBool("villageFishOn", fishOn);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);

        checkCompletion = profile.GetBool("villageCompletion");
        fishOn = profile.GetBool("villageFishOn");

        if (checkCompletion)
            gridAnimator.ChangeMovementDuration(0.5f);
    }


    public void OnHouseExit()
    {
        if (!SaveSystem.Current.GetBool(INTRO_CUTSCENE_SAVE_STRING))
        {
            SaveSystem.Current.SetBool(INTRO_CUTSCENE_SAVE_STRING, true);
            StartCoroutine(StartVillageCutscene());
        }
    }

    private IEnumerator StartVillageCutscene()
    {
        introCameraDolly.StartTrack();

        yield return null;
    }

    public void OnWaterfallEntry()
    {
        // if puzzle complete + enter waterfall, then mark the cave door as exploded
        if (PlayerInventory.Contains("Slider 9", Area.Village))
        {
            SaveSystem.Current.SetBool("caveDoorExploded", true);
        }
    }

    // === Village puzzle specific ===
    
    public void CheckFishOn(Condition c)
    {
        c.SetSpec(fishOn);
    }

    public void TurnFishOn()
    {
        if (!fishOn)
        {
            fishOn = true;
            particleSpawner.GetComponent<ParticleSpawner>().SetFishOn();
        }
    }

    // Puzzle 8 - 8puzzle
    private void CheckHole()
    {
        if (SaveSystem.Current.GetBool("villageHoleFilled"))
        {
            ruinsFragment.SetActive(false);
        }
        else if (PlayerInventory.Contains("Slider 3", Area.Caves)) // if they finish caves before village
        {
            ruinsFragment.transform.SetParent(slider8FloorTransform);
            ruinsFragment.transform.position = slider8FloorTransform.position;
            ruinsFragment.GetComponent<Collider2D>().enabled = true;
        }
    }

    public void FillInHole()
    {
        if (!SaveSystem.Current.GetBool("villageHoleFilled"))
        {
            SaveSystem.Current.SetBool("villageHoleFilled", true);

            AudioManager.Play("Puzzle Complete");

            Item ruinsFrag = PlayerInventory.RemoveItem();
            ruinsFrag.gameObject.SetActive(false);
            
            ruinsSymbols.ruinsHole.enabled = false;
            ruinsSymbols.SetSprites(false);

            ParticleManager.SpawnParticle(ParticleType.SmokePoof, ruinsSymbols.ruinsHole.transform.position, Quaternion.identity, ruinsSymbols.transform);

            AchievementManager.SetAchievementStat("completedVillage", 1);
            if (SaveSystem.Current.GetPlayTimeInSeconds() < 180)
            {
                AchievementManager.SetAchievementStat("completedVillageSpeedrun", 1);
            }
        }
    }

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
        ruinsSymbols.FlashSymbol(0);

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(0.25f, 0.25f);
        AudioManager.Play("Slide Rumble");
        ruinsSymbols.FlashSymbol(1);

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(0.75f, 0.5f);
        AudioManager.Play("Slide Rumble");
        ruinsSymbols.FlashSymbol(2);

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(1.5f, 2.5f);
        AudioManager.PlayWithVolume("Slide Explosion", 0.2f);
        AudioManager.Play("TFT Bell");
        ruinsSymbols.FlashSymbol(3);

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
            Debug.LogError("Tried to shuffle village when not 8 tiles were collected! Detected " + GetNumTilesCollected() + " tiles.");
            return;
        }

        int[,] shuffledPuzzle = new int[3, 3] { { 7, 0, 1 },
                                                { 6, 4, 8 },
                                                { 5, 3, 2 } };
        SetGrid(shuffledPuzzle);

        gridAnimator.ChangeMovementDuration(0.5f);

        checkCompletion = true;
        SaveSystem.Current.SetBool("forceAutoMove", true);
        SaveSystem.Current.SetBool("villageCompletion", checkCompletion);

        OnGridMove += UpdateButtonCompletions; // this is probably not needed
        UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
    }

    protected override void UpdateButtonCompletionsHelper()
    {
        base.UpdateButtonCompletionsHelper();

        CheckFinalPlacements(UIArtifact.GetGridString());
    }

    private void CheckFinalPlacements(string gridString)
    {
        if (!PlayerInventory.Contains("Slider 9", myArea) && gridString == "624_8#7_153" && placeTile9Coroutine == null)
        {
            AudioManager.Play("Puzzle Complete");

            // Disable artifact movement
            UIArtifact.DisableMovement(false);
            SaveSystem.Current.SetBool("forceAutoMove", false);

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
        
        placeTile9Coroutine = null;
    }

    public void Explode()
    {
        caveDoorEntrance.SetActive(true);
        caveDoorRocks.SetActive(false);
        SaveSystem.Current.SetBool("caveDoorExploded", true);
        CameraShake.Shake(1f, 3.5f);
        AudioManager.Play("Slide Explosion");
    }
}
