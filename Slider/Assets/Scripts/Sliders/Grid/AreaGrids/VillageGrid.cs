using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageGrid : SGrid
{
    public static VillageGrid instance;

    public GameObject caveDoorEntrance;
    public GameObject caveDoorRocks;
    public GameObject particleSpawner;

    private bool fishOn;

    private Coroutine shuffleBuildUpCoroutine;
    private bool checkCompletion = false; // TODO: serialize

    public override void Init() {
        myArea = Area.Village;

        foreach (Collectible c in collectibles)
        {
            c.SetArea(myArea);
        }

        base.Init();

        if (fishOn)
        {
            particleSpawner.GetComponent<ParticleSpawner>().SetFishOn();
        }
        instance = this;
    }

    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("Village");
        UIEffects.FadeFromBlack();
        
        if (checkCompletion) {
            UpdateButtonCompletions(this, null);
        }
    }
    
    private void OnEnable() {
        if (checkCompletion) {
            Debug.Log("OnEnable checkCompletion");
            SGrid.OnGridMove += SGrid.UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
            SGridAnimator.OnSTileMoveEnd += CheckFinalPlacementsOnMove;
        }
    }

    private void OnDisable() {
        if (checkCompletion) {
            SGrid.OnGridMove -= SGrid.UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract -= SGrid.UpdateButtonCompletions;
            SGridAnimator.OnSTileMoveEnd -= CheckFinalPlacementsOnMove;
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
    }


    // === Village puzzle specific ===
    public void CheckFishOn(Conditionals.Condition c)
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
    public void ShufflePuzzle() {
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

        CameraShake.Shake(0.75f, 0.5f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(1f);

        CameraShake.Shake(1.5f, 2.5f);
        AudioManager.Play("Slide Explosion");

        yield return new WaitForSeconds(0.25f);

        UIEffects.FlashWhite();
        DoShuffle();
    }

    private void DoShuffle()
    {
        int[,] shuffledPuzzle = new int[3, 3] { { 7, 0, 1 },
                                                { 6, 4, 8 },
                                                { 5, 3, 2 } };
        SetGrid(shuffledPuzzle);

        checkCompletion = true;
        OnGridMove += UpdateButtonCompletions; // this is probably not needed
        UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
        SGridAnimator.OnSTileMoveEnd += CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletions
    }


    private void CheckFinalPlacementsOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (!PlayerInventory.Contains("Slider 9", Area.Village) && (GetGridString() == "624_8#7_153"))
        {
            GivePlayerTheCollectible("Slider 9");

            // Disable queues
            UIArtifact.ClearQueues();

            // we don't have access to the Collectible.StartCutscene() pick up, so were doing this dumb thing instead
            StartCoroutine(CheckCompletionsAfterDelay(1.1f));

            AudioManager.Play("Puzzle Complete");
            UIArtifactWorldMap.SetAreaStatus(Area.Village, ArtifactWorldMapArea.AreaStatus.color);
        }
    }

    public void Explode()
    {
        caveDoorEntrance.SetActive(true);
        caveDoorRocks.SetActive(false);
        CameraShake.Shake(1f, 3.5f);
        AudioManager.Play("Slide Explosion");
    }
}
