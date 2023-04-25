using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class OceanGrid : SGrid
{
    public GameObject burriedGuyNPC;
    public GameObject burriedTreasure;
    public ParticleSystem burriedTreasureParticles;
    public KnotBox knotBox;
    private bool knotBoxEnabledLastFrame;
    public BottleManager bottleManager;
    public NPCRotation npcRotation;
    public OceanArtifact oceanArtifact; // used for the final quest to lock movement
    public GameObject treesToJungle;
    public List<int> buoytiles = new List<int> {1, 3, 4, 8, 9};
    public OceanDolly introCameraDolly;
    private const string INTRO_CUTSCENE_SAVE_STRING = "oceanIntroCutscene";

    private Vector2Int[] correctPath =
    {
        Vector2Int.left,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.left,
    };
    private int playerIndex = 0;
    private Vector2Int playerMovement;
    private int lastIslandId = 1;
    private bool foggyCompleted;
    // public GameObject fog6;
    // public GameObject fog7;
    public FogAnimationController fogAnimationController6;
    public FogAnimationController fogAnimationController7;
    public GameObject fogIsland;
    private int fogIslandId; //tile which fog island was found on
    
    [SerializeField] private Volcano volcano;

    private bool isCompleted = false;

    [Header("Foggy Progress Notes")]
    [SerializeField] private List<SpriteRenderer> progressNotes;
    [SerializeField] private Sprite emptyNote, fullNote;
    [SerializeField] private GameObject sparklePrefab;

    public override void Init()
    {
        InitArea(Area.Ocean);
        base.Init();
        
    }


    protected override void Start()
    {
        base.Start();
        burriedGuyNPC.SetActive(false);
        fogIsland.SetActive(false);

        AudioManager.PlayMusic("Ocean");
        AudioManager.PlayMusic("Ocean Tavern", false); // for FMOD effects
        AudioManager.PlayMusic("Ocean uwu", false); // for FMOD effects

        foreach (SpriteRenderer note in progressNotes)
        {
            note.enabled = false;
        }
        Pan();

    }

    private void OnEnable()
    {
        if (checkCompletion && !isCompleted)
        {
            UpdateButtonCompletions(this, null);
            OnGridMove += UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
            SGridAnimator.OnSTileMoveEnd += CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletions
            UIArtifactMenus.OnArtifactOpened += CheckFinalPlacementsOnMove;
        }

        SGridAnimator.OnSTileMoveEnd += CheckShipwreck;
        UIArtifactMenus.OnArtifactOpened += CheckShipwreck;
        SGridAnimator.OnSTileMoveEnd += CheckVolcano;
        UIArtifactMenus.OnArtifactOpened += CheckVolcano;

        SGridAnimator.OnSTileMoveEnd += BuoySuccessEffectsCheck;
    }

    private void OnDisable()
    {
        if (checkCompletion && !isCompleted)
        {
            OnGridMove -= UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract -= SGrid.UpdateButtonCompletions;
            SGridAnimator.OnSTileMoveEnd -= CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletions
            UIArtifactMenus.OnArtifactOpened -= CheckFinalPlacementsOnMove;
        }

        SGridAnimator.OnSTileMoveEnd -= CheckShipwreck;
        UIArtifactMenus.OnArtifactOpened -= CheckShipwreck;
        SGridAnimator.OnSTileMoveEnd -= CheckVolcano;
        UIArtifactMenus.OnArtifactOpened -= CheckVolcano;
        
        SGridAnimator.OnSTileMoveEnd -= BuoySuccessEffectsCheck;
    }

    private void Update()
    {
        UpdateRomeoReason();
        
        //Get tile the player is on. if it change from last update, find the direction the player moved. Add direction to list of moves. put list in checkfoggy. only why on fog tiles
        UpdatePlayerMovement();
    }

    private void UpdateRomeoReason()
    {
        string reason = "The path is obstructed!";
        string gridString = GetGridString();
        
        if (gridString[0] == '2' || gridString[0] == '8')
        {
            reason = "There is land in the way!";
        }
        else if (gridString[0] == '4')
        {
            reason = "The shipwreck is in the way!";
        }
        else if (gridString[0] == '5')
        {
            reason = "The island is in the way!";
        }
        else if (gridString[0] == '9')
        {
            reason = "The volcano is in the way!";
        }
        else if (gridString[0] == '.')
        {
            reason = "The space in front of me is empty!";
        }

        SaveSystem.Current.SetString("oceanRomeoReason", reason);
    }
    
    private void LateUpdate() {
        knotBoxEnabledLastFrame = knotBox.isActiveAndEnabled;
    }

    public override void Save()
    {
        base.Save();
        SaveSystem.Current.SetBool("oceanRJBottleDelivery", bottleManager.puzzleSolved);
        SaveSystem.Current.SetBool("oceanFoggyIslandReached", foggyCompleted);
        SaveSystem.Current.SetBool("oceanUnlockedAllSliders", npcRotation.unlockedAllSliders);
        SaveSystem.Current.SetBool("oceanBreadgeCollected", npcRotation.gotBreadge);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        checkCompletion = profile.GetBool("oceanCompletion");
        isCompleted = profile.GetBool("oceanCompleted");
        if (isCompleted) ((OceanArtifact)OceanArtifact._instance).SetCanRotate(false);

        treesToJungle.SetActive(!profile.GetBool("oceanTreesRemoved"));

        bottleManager.puzzleSolved = profile.GetBool("oceanRJBottleDelivery");
        foggyCompleted = profile.GetBool("oceanFoggyIslandReached");
        npcRotation.unlockedAllSliders = profile.GetBool("oceanUnlockedAllSliders");
        npcRotation.gotBreadge = profile.GetBool("oceanBreadgeCollected");

    }

    public override void EnableStile(STile stile, bool shouldFlicker = true)
    {
        if(stile.islandId == 2 && !stile.isTileActive)
        {
            CheckTile2Placement(stile);
        }
        if (stile.islandId == 3 && !stile.isTileActive)
        {
            CheckTile3Placement(stile);
        }

        base.EnableStile(stile, shouldFlicker);

        stile.GetComponentInChildren<SpriteMask>().enabled = false; // on STile/SlideableArea

        if (grid != null)
        {
            CheckShipwreck(this, null);
            CheckVolcano(this, null);
        }
    }

    private void CheckTile2Placement(STile stile)
    {
        int stile1x = GetStile(1).x;
        int stile1y = GetStile(1).y;

        if (stile1x < 2)
        {
            if (stile1y < 2)
            {
                if (CheckTile2PlacementHelper(stile, stile1x + 1, stile1y + 1))
                    return;
            }
            else if (stile1y > 0)
            {
                if (CheckTile2PlacementHelper(stile, stile1x + 1, stile1y - 1))
                    return;
            }
        }
        else if (stile1x > 0)
        {
            if (stile1y < 2)
            {
                if (CheckTile2PlacementHelper(stile, stile1x - 1, stile1y + 1))
                    return;
            }
            else if (stile1y > 0)
            {
                if (CheckTile2PlacementHelper(stile, stile1x - 1, stile1y - 1))
                    return;
            }
        }

        // wah wah u couldnt swap it for some reason boohoo
    }

    private bool CheckTile2PlacementHelper(STile slider2, int x, int y)
    {
        STile other = GetStileAt(x, y);
        if (other.islandId != 1)
        {
            SwapTiles(slider2, other);
            return true;
        }
        return false;
    }

    private void CheckTile3Placement(STile stile)
    {
        //try to spawn on middle right
        STile other = GetStileAt(2, 1);
        if (other.islandId != 1 && other.islandId != 2)
        {
            SwapTiles(stile, other);
            return;
        }
        else if(other.islandId == 2) //middle right taken up by tavern so try middle else just ff lol
        {
            STile midddle_tile = GetStileAt(1, 1);
            if(midddle_tile.islandId == 1)
            {
                SwapTiles(stile, GetStileAt(2,2));
                return;
            }
        }
        else //middle right is taken up by start so try middle middle or left middle
        {
            STile midddle_tile = GetStileAt(1, 1);
            if(midddle_tile.islandId == 1)
            {
                SwapTiles(stile, GetStileAt(0,1));
                return;
            }
            else
                SwapTiles(stile, midddle_tile);
        }
    }



    // === Ocean puzzle specific ===

    public void CheckShipwreck(object sender, System.EventArgs e)
    {
        if (IsShipwreckAdjacent())
        {
            if (!PlayerInventory.Contains("Treasure Chest"))
            {
                burriedTreasure.SetActive(true);
                burriedTreasureParticles.Play();
            }
        }
        else
        {
            burriedTreasure.SetActive(false);
        }
    }

    public bool IsShipwreckAdjacent()
    {
        return CheckGrid.contains(GetGridString(), "41");
    }

    public void CheckVolcano(object sender, System.EventArgs e)
    {
        if (IsVolcanoSet())
        {
            if (!SaveSystem.Current.GetBool("oceanVolcanoErupted"))
            {
                AudioManager.Play("Puzzle Complete");

                volcano.Erupt();
            }
        }
    }

    public bool IsVolcanoSet()
    {
        return CheckGrid.contains(GetGridString(), ".4._895_.3.");
    }

    public void ActivateBurriedNPC()
    {
        burriedGuyNPC.SetActive(true);
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, burriedGuyNPC.transform.position, burriedGuyNPC.transform);
    }

    public void SpawnFezziwigReward() {
        Collectible c = GetCollectible("Magical Gem");
        if (!PlayerInventory.Contains(c)) {
            c.gameObject.SetActive(true);
            AudioManager.Play("Puzzle Complete");
        }
    }

    private bool BuoyConditions()
    {
        return (
            AllBuoy() && 
            knotBox.isActiveAndEnabled && 
            knotBoxEnabledLastFrame &&
            knotBox.CheckLines() == 0
        );
    }

    private void BuoySuccessEffectsCheck(object sender, System.EventArgs e)
    {
        if (PlayerInventory.Contains("Golden Fish"))
            return;

        if (BuoyConditions())
        {
            knotBox.CheckParticles();
            AudioManager.Play("Puzzle Complete");
        }
    }

    public void BuoyAllFound(Condition c)
    {
        c.SetSpec(AllBuoy());
    }

    //C: Returns if all the required buoy tiles are active
    public bool AllBuoy()
    {
        return SGrid.AreTilesActive(GetStiles(buoytiles));
    }

    public void knotBoxEnabled(Condition c)
    {
        c.SetSpec(knotBox.isActiveAndEnabled && AllBuoy());
    }

    public void knotBoxDisabled(Condition c)
    {
        c.SetSpec(!knotBox.isActiveAndEnabled && AllBuoy());
    }

    public void BuoyCheck(Condition c)
    {
        c.SetSpec(BuoyConditions());
    }

    public void ToggleKnotBox()
    {
        if (AllBuoy())
        {
            knotBox.enabled = !knotBox.enabled;
            knotBox.CheckLines();
            if (knotBox.enabled)
            {
                foreach (GameObject knotnode in knotBox.knotnodes)
                {
                    UITrackerManager.AddNewTracker(knotnode, UITrackerManager.DefaultSprites.circle2);
                }
            }
            else
            {
                foreach (GameObject knotnode in knotBox.knotnodes)
                {
                    UITrackerManager.RemoveTracker(knotnode);
                }
            }
        }
    }

    //public void IsLostGuyBeached(Condition c)
    //{
    //    c.SetSpec(lostGuyMovement.hasBeached);
    //}

    // Foggy Seas

    public void CheckFoggySeas(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        bool correct = FoggyCorrectMovement();
        
        if (GetStile(6).isTileActive && GetStile(7).isTileActive && foggyCompleted)
        {
            if(Player.GetInstance().GetSTileUnderneath().islandId == fogIslandId && correct) 
            {
                STile playerStile = Player.GetInstance().GetSTileUnderneath();
                fogIsland.transform.position = playerStile.transform.position;
                fogIsland.transform.SetParent(playerStile.transform);
                fogIsland.SetActive(true);
                SetProgressRingActive(false);

                if (fogIslandId == 6)
                {
                    // fog6.SetActive(false);
                    fogAnimationController6.SetIsVisible(false);
                }
                else
                {
                    // fog7.SetActive(false);
                    fogAnimationController7.SetIsVisible(false);
                }
            }
        }
        else
        {
            fogAnimationController6.SetIsVisible(true);
            fogAnimationController7.SetIsVisible(true);
            // fog6.SetActive(true);
            // fog7.SetActive(true);
        }
        
    }

    private void UpdatePlayerMovement()
    {

        if (Player.GetInstance().GetSTileUnderneath() == null)
        {
            if (lastIslandId == 6 || lastIslandId == 7)
            {
                lastIslandId = 1;
                failFoggy();
                SetProgressRingActive(false);
            }
            return;
        }

        int currentIslandId = Player.GetInstance().GetSTileUnderneath().islandId;
        if ((currentIslandId == 6 || currentIslandId == 7) && !foggyCompleted)
        {
            SetProgressRingActive(true);
        }
        if (currentIslandId != lastIslandId && (lastIslandId == 6 || lastIslandId == 7))
        {

            // fog7.SetActive(true);
            // fog6.SetActive(true);
            fogAnimationController6.SetIsVisible(true);
            fogAnimationController7.SetIsVisible(true);
            fogIsland.SetActive(false);
            //SetProgressRingActive(true);

            if (currentIslandId != 6 && currentIslandId != 7)
            {
                failFoggy();
                SetProgressRingActive(false);
            }
            else
            {
                STile current = GetStile(currentIslandId);
                STile old = GetStile(lastIslandId);

                Vector2Int currentPos = new Vector2Int(current.x, current.y);
                Vector2Int oldPos = new Vector2Int(old.x, old.y);

                playerMovement = currentPos - oldPos;
                CheckFoggySeas(this, null);
            }
        }


        lastIslandId = currentIslandId;

    }

    private void FoggySeasAudio()
    {
        AudioManager.PlayWithPitch("Puzzle Complete", 0.5f + playerIndex * 0.1f);
        AudioManager.SetGlobalParameter("OceanFoggyProgress", 0);
    }

    public bool FoggyCorrectMovement()
    {
        // Debug.Log("player index: " + playerIndex+ " correct path: " + correctPath.Length);
        if(playerIndex == correctPath.Length - 1 && !foggyCompleted && correctPath[playerIndex] == playerMovement)
        {
            playerIndex++;
            FoggyCompleted();
            return true; //only returns true the first time u complete this puzzle basically
        }
        else if (0 <= playerIndex && playerIndex < correctPath.Length - 1 && correctPath[playerIndex] == playerMovement)
        {
            playerIndex++;
            FoggySeasAudio();
            progressNotes[playerIndex - 1].sprite = fullNote;
            return false;
        }
        else
        {
            failFoggy();
            return false;
        }

    }

    private void failFoggy()
    {
        if (playerIndex != 0 && playerIndex != 6)
        {
            AudioManager.Play("Artifact Error");
        }

        playerIndex = 0;
        foggyCompleted = false; // DC: idk why we made it only completable once
        AudioManager.SetGlobalParameter("OceanFoggyProgress", 0);
        foreach (SpriteRenderer note in progressNotes)
        {
            if (note.sprite.Equals(fullNote))
            {
                Instantiate(sparklePrefab, note.gameObject.transform.position, Quaternion.identity);
            }
            note.sprite = emptyNote;
        }
    }

    private void FoggyCompleted()
    {
        foggyCompleted = true;
        fogIslandId = Player.GetInstance().GetSTileUnderneath().islandId;
        for(int i =0; i < correctPath.Length; i++)
            progressNotes[i].sprite = emptyNote;
    }

    private void SetProgressRingActive(bool active)
    {
        foreach (SpriteRenderer note in progressNotes)
        {
            note.enabled = active;
            if (!active)
            {

                Instantiate(sparklePrefab, note.gameObject.transform.position, Quaternion.identity);
            }
        }
    }

    // Final puzzle

    public void IsCompleted(Condition c)
    {
        c?.SetSpec(checkCompletion && IsFinalPuzzleMatching());
    }

    private bool IsFinalPuzzleMatching()
    {
        return CheckGrid.contains(GetGridString(), "412_[^1248]{2}8_[^1248]{3}");
    }

    public bool GetCheckCompletion()
    {
        return checkCompletion;
    }

    public bool GetIsCompleted()
    {
        return isCompleted;
    }

    public void StartFinalChallenge()
    {
        if (!checkCompletion)
        {
            checkCompletion = true;
            SaveSystem.Current.SetBool("oceanCompletion", checkCompletion);

            OnGridMove += UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
            SGridAnimator.OnSTileMoveEnd += CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletions
            UIArtifactMenus.OnArtifactOpened += CheckFinalPlacementsOnMove;

            SGrid.UpdateButtonCompletions(this, null);
        }
    }

    private void CheckFinalPlacementsOnMove(object sender, System.EventArgs e)
    {
        if (!isCompleted && IsFinalPuzzleMatching())
        {
            isCompleted = true;
            SaveSystem.Current.SetBool("oceanCompleted", checkCompletion);
            ((OceanArtifact)OceanArtifact._instance).SetCanRotate(false);

            AudioManager.Play("Puzzle Complete");
            oceanArtifact.FlickerAllOnce();

            UIArtifactWorldMap.SetAreaStatus(Area.Ocean, ArtifactWorldMapArea.AreaStatus.color);

            StartCoroutine(ShowMapAfterDelay(1));
        }
    }

    private IEnumerator ShowMapAfterDelay(float t)
    {
        yield return new WaitForSeconds(t);

        UIArtifactMenus._instance.OpenArtifactAndShow(2);
    }

    protected override void UpdateButtonCompletionsHelper()
    {
        for (int x = 0; x < Current.Width; x++)
        {
            for (int y = 0; y < Current.Width; y++)
            {
                // int tid = current.targetGrid[x, y];
                string tids = GetTileIdAt(x, y);
                ArtifactTileButton artifactButton = UIArtifact.GetButton(x, y);
                if (tids == "*")
                {
                    int abid = artifactButton.islandId;
                    bool isLand = abid == 1 || abid == 2 || abid == 4 || abid == 8; // this is scuffed

                    // UIArtifact.SetButtonComplete(current.grid[x, y].islandId, true);
                    UIArtifact.SetButtonComplete(artifactButton.islandId, !isLand);
                    UIArtifact.GetButton(artifactButton.x, artifactButton.y).SetHighlighted(isLand);
                }
                else
                {
                    int tid = int.Parse(tids);
                    // UIArtifact.SetButtonComplete(tid, current.grid[x, y].islandId == tid);
                    UIArtifact.SetButtonComplete(artifactButton.islandId, artifactButton.islandId == tid);
                    UIArtifact.GetButton(artifactButton.x, artifactButton.y).SetHighlighted(artifactButton.islandId != tid);
                }
            }
        }
    }

    public void ClearTreesToJungle() // called in ShopDialogueManager
    {
        SaveSystem.Current.SetBool("oceanTreesRemoved", true);
        treesToJungle.SetActive(false);
        CameraShake.Shake(1, 2);
        AudioManager.Play("Slide Explosion");
    }

    public void Pan()
    {
        if (!SaveSystem.Current.GetBool(INTRO_CUTSCENE_SAVE_STRING))
        {
            SaveSystem.Current.SetBool(INTRO_CUTSCENE_SAVE_STRING, true);
            introCameraDolly.StartTrack();
        }
        else
        {
            introCameraDolly.intern1.gameObject.SetActive(false);
            introCameraDolly.intern2.gameObject.SetActive(false);
        }
    }
}
