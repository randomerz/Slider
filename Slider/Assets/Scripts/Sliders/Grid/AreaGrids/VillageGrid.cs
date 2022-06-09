using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageGrid : SGrid
{
    public static VillageGrid instance;

    public GameObject caveDoorEntrance;
    public GameObject caveDoorRocks;
    public GameObject particleSpawner;

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

    private Coroutine shuffleBuildUpCoroutine;
    private bool checkCompletion = false; // TODO: serialize

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
            Debug.Log("OnEnable checkCompletion");
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

    // Mini-Puzzle - Chad Flashlight
    public void CheckChadMoved(object sender, SGridAnimator.OnTileMoveArgs e) {
        if (GetStile(8).isTileActive && e.stile.islandId == 8 && !chadFell && chadMet) {
            chadFell = true;
            chad.transform.GetChild(0).GetComponent<Animator>().SetBool("isFalling", true);
            StartCoroutine(ChadFall());
        }
    }

    public void ChadPickUpFlashLight() {
        if (!chadMet) {
            chadMet = true;
            Transform pickUpLocation = new GameObject().transform;
            pickUpLocation.position = chad.transform.position + flashlightPadding;
            flashlight.GetComponent<Item>().PickUpItem(pickUpLocation, callback:AfterChadPickup);
        }
    }
    private void AfterChadPickup() {
        oldFlashParent = flashlight.transform.parent;
        flashlight.transform.parent = chadPickUpPoint.transform;
        //StartCoroutine(ChadJump());
    }

    // So that the dcond can call after dialogue ends
    public void ChadJumpStarter() {
        if (!chadJumped) {
            StartCoroutine(ChadJump());
            chadJumped = !chadJumped;
        }
    }

    // Animates Chad Jumping
    public IEnumerator ChadJump() {
        var chadform = chad.transform;
        var chadimator = chadform.GetChild(0).GetComponent<Animator>();
        var target = new GameObject().transform;
        target.position = chadform.position + Vector3.left + Vector3.up;
        chadimator.SetBool("isJumping", true);

        float t = 0;

        Vector3 start = new Vector3(chadform.position.x, chadform.position.y);
        while (t < jumpDuration)
        {
            float x = xJumpMotion.Evaluate(t / jumpDuration);
            float y = yJumpMotion.Evaluate(t / jumpDuration);
            Vector3 pos = new Vector3(Mathf.Lerp(start.x, target.transform.position.x, x),
                                      Mathf.Lerp(start.y, target.transform.position.y, y));
            
            chadRenderer.transform.position = pos;

            yield return null;
            t += Time.deltaTime;
        }

        chadform.position = target.position;
        chadRenderer.transform.position = target.position;
        chadllider.enabled = false;

        chadimator.SetBool("isJumping", false);
        yield return null;
    }

    // Animates Chad Falling
    private IEnumerator ChadFall() {
        var chadform = chad.transform;
        var chadimator = chadform.GetChild(0).GetComponent<Animator>();
        var target = new GameObject().transform;
        target.position = chadform.position + Vector3.left + Vector3.down;
        chadimator.SetBool("isFalling", true);

        float t = jumpDuration;

        //Create 2 dummy transforms for the animation.
        GameObject start = new GameObject();
        start.transform.position = chadform.position;
        GameObject end = new GameObject();
        end.transform.position = target.position;

        //transform.position = target;
        while (t >= 0)
        {
            float x = xJumpMotion.Evaluate(t / jumpDuration);
            float y = yJumpMotion.Evaluate(t / jumpDuration);
            Vector3 pos = new Vector3(Mathf.Lerp(end.transform.position.x, start.transform.position.x, x),
                                      Mathf.Lerp(end.transform.position.y, start.transform.position.y, y));
            
            chadRenderer.transform.position = pos;

            yield return null;
            t -= Time.deltaTime;
        }

        chadllider.enabled = true;
        chadform.position = end.transform.position;
        chadRenderer.transform.position = end.transform.position;


        chadimator.SetBool("isFallen", true);
        chadimator.SetBool("isFalling", false);
        
        flashlight.transform.parent = oldFlashParent;
        flashlight.GetComponent<Item>().DropItem(chad.transform.position + (Vector3.left * 1.5f), callback:ChadFinishFall);
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
