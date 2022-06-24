using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageGrid : SGrid
{
    public static VillageGrid instance;

    public GameObject caveDoorEntrance;
    public GameObject caveDoorRocks;
    public GameObject particleSpawner;

    [Header("Chad Animation stuff")]
    [SerializeField] private GameObject chad;
    [SerializeField] private GameObject flashlight;
    [SerializeField] private AnimationCurve xJumpMotion;
    [SerializeField] private AnimationCurve yJumpMotion;
    [SerializeField] private SpriteRenderer chadRenderer;
    [SerializeField] private SpriteRenderer flashRenderer;
    [SerializeField] private float jumpDuration;
    [SerializeField] private GameObject chadPickUpPoint;
    private Collider2D chadllider;
    private Transform oldFlashParent;
    private Vector3 flashlightPadding;
    private bool chadJumped;
    private bool chadFell;
    private bool chadMet;

    private bool fishOn;

    [SerializeField] private RuinsSymbols ruinsSymbols;
    private Coroutine shuffleBuildUpCoroutine;
    private bool checkCompletion = false;

    public override void Init() {
        myArea = Area.Village;

        foreach (Collectible c in collectibles)
        {
            c.SetArea(myArea);
        }

        base.Init();

        // === Chad Stuff ===
        chadFell = false;
        chadMet = false;
        chadJumped = false;
        flashlightPadding = Vector3.up * 0.5f;
        // Finds the non-trigger collider in chad
        var colliders = chad.GetComponents(typeof(Collider2D));
        chadllider = ((Collider2D)colliders[0]).isTrigger? (Collider2D)colliders[1] : (Collider2D)colliders[0];
        // === Chad Stuff ===

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

        // Set the flashlight collider to be disabled from the beginning so that the player can't collect it
        flashlight.GetComponent<Item>().SetCollider(false);
    }
    
    private void OnEnable() {
        SGridAnimator.OnSTileMoveEnd += CheckChadMoved;
        if (checkCompletion) {
            SGrid.OnGridMove += SGrid.UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
            SGridAnimator.OnSTileMoveEnd += CheckFinalPlacementsOnMove;
        }
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveEnd -= CheckChadMoved;
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

        if (checkCompletion)
            gridAnimator.ChangeMovementDuration(0.5f);
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
        AudioManager.Play("Slide Explosion");
        ruinsSymbols.FlashSymbol(3);

        yield return new WaitForSeconds(0.25f);

        UIEffects.FlashWhite();
        DoShuffle();

        yield return new WaitForSeconds(0.75f);

        CameraShake.Shake(2, 0.9f);
    }

    private void DoShuffle()
    {
        int[,] shuffledPuzzle = new int[3, 3] { { 7, 0, 1 },
                                                { 6, 4, 8 },
                                                { 5, 3, 2 } };
        SetGrid(shuffledPuzzle);

        gridAnimator.ChangeMovementDuration(0.5f);
        SettingsManager.ForceAutoMove = true;

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
            StartCoroutine(CheckCompletionsAfterDelay(1.2f));

            AudioManager.Play("Puzzle Complete");
            SettingsManager.ForceAutoMove = false;
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

    // Mini-Puzzle - Chad Flashlight
    public void CheckChadMoved(object sender, SGridAnimator.OnTileMoveArgs e) {
        if (GetStile(8).isTileActive && e.stile.islandId == 8 && !chadFell && chadMet && chadJumped) {
            chadFell = true;
            chad.transform.GetChild(0).GetComponent<Animator>().SetBool("isTipping", true);
            StartCoroutine(ChadFall());
        }
    }

    public void ChadPickUpFlashLight() {
        if (!chadMet) {
            chadMet = true;
        }
    }

    // So that the dcond can call after dialogue ends
    public void ChadJumpStarter() {
        if (!chadJumped) {
            StartCoroutine(ChadJump());
        }
    }

    // Animates Chad Jumping
    public IEnumerator ChadJump() {
        var chadform = chad.transform;
        var chadimator = chadform.GetChild(0).GetComponent<Animator>();
        var target = new GameObject().transform;
        target.parent = GetStile(8).transform;
        target.localPosition = chadform.localPosition + new Vector3(1.5f, 1);
        chadimator.SetBool("isJumping", true);

        float t = 0;

        Vector3 start = new Vector3(chadform.localPosition.x, chadform.localPosition.y);
        while (t < jumpDuration)
        {
            float x = xJumpMotion.Evaluate(t / jumpDuration);
            float y = yJumpMotion.Evaluate(t / jumpDuration);
            Vector3 pos = new Vector3(Mathf.Lerp(start.x, target.transform.localPosition.x, x),
                                      Mathf.Lerp(start.y, target.transform.localPosition.y, y));
            
            chadform.localPosition = pos;

            yield return null;
            t += Time.deltaTime;
        }

        //chadform.localPosition = target.localPosition;
        chadllider.enabled = false;

        chadimator.SetBool("isJumping", false);
        chadJumped = true;
        yield return null;
    }

    // Animates Chad Falling
    private IEnumerator ChadFall() {
        var chadform = chad.transform;
        var chadimator = chadform.GetChild(0).GetComponent<Animator>();
        var target = new GameObject().transform;
        target.localPosition = chadform.localPosition + new Vector3(1f, -1);
        chadimator.SetBool("isTipping", true);

        yield return new WaitForSeconds(0.5f);

        chadimator.SetBool("isFallen", true);
        AudioManager.Play("Fall");
        Vector3 startPos = chad.transform.localPosition;
        Vector3 targetPos = target.transform.localPosition;

        float fallDuration = 0.5f;
        float t = 0;
        while (t < fallDuration) {
            chadform.localPosition = Vector3.Lerp(startPos, targetPos, t / fallDuration);

            yield return null;
            t += Time.deltaTime;
        }

        chadimator.SetBool("isTipping", false);
        AudioManager.Play("Hurt");
        chadform.localPosition = targetPos;

        flashlight.transform.parent = GetStile(8).transform;
        flashlight.GetComponent<Item>().DropItem(chad.transform.position + (Vector3.right * 1f), callback: ChadFinishFall);
    }
    private void ChadFinishFall() {
        var flashlight_item = flashlight.GetComponent<Item>();
        flashlight_item.SetCollider(true);
        chadllider.enabled = true;
    }

    public void ChadFell(Conditionals.Condition cond) {
        cond.SetSpec(chadFell);
    }
}
