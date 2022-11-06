using System.Collections.Generic;
using UnityEngine;

public class OceanGrid : SGrid
{
    // public static OceanGrid instance;

    private static bool checkCompletion = false; // TODO: serialize all these
    private static bool isCompleted = false;
    public static bool canRotate = true; // global variable, used in OceanArtifact

    public GameObject burriedGuyNPC;
    public KnotBox knotBox;
    public BottleManager bottleManager;
    public OceanArtifact oceanArtifact; // used for the final quest to lock movement
    public GameObject treesToJungle;
    public List<int> buoytiles = new List<int> {1, 3, 4, 8, 9};

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
    public GameObject fog6;
    public GameObject fog7;
    public GameObject fogIsland;
    private int fogIslandId; //tile which fog island was found on

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
        UIEffects.FadeFromBlack();

        foreach (SpriteRenderer note in progressNotes)
        {
            note.enabled = false;
        }

    }

    private void OnEnable()
    {
        if (checkCompletion)
        {
            UpdateButtonCompletions(this, null);
            OnGridMove += UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
            SGridAnimator.OnSTileMoveEnd += CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletions
        }

        SGridAnimator.OnSTileMoveEnd += CheckShipwreck;
        SGridAnimator.OnSTileMoveEnd += CheckVolcano;
    }

    private void OnDisable()
    {
        if (checkCompletion)
        {
            OnGridMove -= UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract -= SGrid.UpdateButtonCompletions;
            SGridAnimator.OnSTileMoveEnd -= CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletions
        }

        SGridAnimator.OnSTileMoveEnd -= CheckShipwreck;
        SGridAnimator.OnSTileMoveEnd -= CheckVolcano;
    }

    private void Update()
    {

        //Get tile the player is on. if it change from last update, find the direction the player moved. Add direction to list of moves. put list in checkfoggy. only why on fog tiles
        updatePlayerMovement();

    }

    public override void Save()
    {
        base.Save();
        SaveSystem.Current.SetBool("OceanRJBottleDelivery", bottleManager.puzzleSolved);
        SaveSystem.Current.SetBool("FoggyIslandReached", foggyCompleted);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        bottleManager.puzzleSolved = SaveSystem.Current.GetBool("RJBottleDelivery");
        foggyCompleted = SaveSystem.Current.GetBool("FoggyIslandReached");
    }

    public override void EnableStile(STile stile, bool shouldFlicker = true)
    {
        if (stile.islandId == 3)
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

    private void CheckTile3Placement(STile stile)
    {
        string s = GetGridString(true);

        if (CheckGrid.contains(s, "31") || CheckGrid.contains(s, "13")
            || CheckGrid.contains(s, "1...3") || CheckGrid.contains(s, "3...1"))
        {
            List<STile> tiles = new List<STile>();
            foreach (STile tile in grid)
            {
                tiles.Add(tile);
            }

            bool firstRun = true;

            STile other = tiles[tiles.Count - 1];
            while (tiles.Count > 0 && ((CheckGrid.contains(s, "31") || CheckGrid.contains(s, "13")
                 || CheckGrid.contains(s, "1...3") || CheckGrid.contains(s, "3...1"))))
            {
                if (!firstRun)
                {
                    tiles.Remove(other);
                }
                firstRun = false;

                for (int i = tiles.Count - 1; i >= 0; i--)
                {
                    other = tiles[i];
                    if (!other.isTileActive)
                    {
                        break;
                    }
                    else
                    {
                        tiles.Remove(other);
                    }
                }

                SwapTiles(stile, other);

                s = GetGridString(true);
            }
        }
    }



    // === Ocean puzzle specific ===

    public void CheckShipwreck(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (IsShipwreckAdjacent())
        {
            Collectible c = GetCollectible("Treasure Chest");

            if (!PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(true);
            }
        }
    }

    public bool IsShipwreckAdjacent()
    {
        return CheckGrid.contains(GetGridString(), "41");
    }

    public void CheckVolcano(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (IsVolcanoSet())
        {
            Collectible c = GetCollectible("Rock");

            if (!PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(true);
                AudioManager.Play("Puzzle Complete");

                // have some particle effects and propper stuff here
                // CameraShake.Shake(0.75f, 5); 
                AudioManager.Play("Slide Explosion");
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
    }

    private bool BuoyConditions()
    {
        return (AllBuoy() && knotBox.isActiveAndEnabled && (knotBox.CheckLines() == 0));
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
            if (knotBox.enabled)
            {
                foreach (GameObject knotnode in knotBox.knotnodes)
                {
                    UITrackerManager.AddNewTracker(knotnode);
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
            if(Player.GetStileUnderneath().islandId == fogIslandId && correct) 
            {
                fogIsland.transform.position = Player.GetStileUnderneath().transform.position;
                fogIsland.SetActive(true);
                if (fogIslandId == 6)
                {
                    fog6.SetActive(false);
                }
                else
                {
                    fog7.SetActive(false);
                }
            }
            
            SetProgressRingActive(false);

            
        }
        else
        {
            fog6.SetActive(true);
            fog7.SetActive(true);
        }
        
    }

    private void updatePlayerMovement()
    {

        if (Player.GetStileUnderneath() == null)
        {
            return;
        }

        int currentIslandId = Player.GetStileUnderneath().islandId;
        if ((currentIslandId == 6 || currentIslandId == 7))
        {
            SetProgressRingActive(true);
        }
        if (currentIslandId != lastIslandId && (lastIslandId == 6 || lastIslandId == 7))
        {

            fog7.SetActive(true);
            fog6.SetActive(true);
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
        AudioManager.PlayWithPitch("Puzzle Complete", 0.3f + playerIndex * 0.05f);
        AudioManager.SetMusicParameter("Ocean", "OceanFoggyProgress", playerIndex);
    }

    public bool FoggyCorrectMovement()
    {
        Debug.Log("player index: " + playerIndex+ " correct path: " + correctPath.Length);
        if(playerIndex == correctPath.Length-1 && !foggyCompleted)
        {
            FoggyCompleted();
            return true;//only returns true the first time u complete this puzzle basically
        }
        else if (playerIndex >=0 && playerIndex < correctPath.Length && correctPath[playerIndex] == playerMovement)
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
        if(playerIndex > 5) //cant fail if u reached the island
            return;
        if (playerIndex != 0)
        {
            AudioManager.Play("Artifact Error");
        }

        playerIndex = 0;
        AudioManager.SetMusicParameter("Ocean", "OceanFoggyProgress", 0);
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
        fogIslandId = Player.GetStileUnderneath().islandId;
        for(int i =0; i < correctPath.Length; i++)
            progressNotes[i].sprite = emptyNote;
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
            OnGridMove += UpdateButtonCompletions; // this is probably not needed
            UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
            SGridAnimator.OnSTileMoveEnd += CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletions

            SGrid.UpdateButtonCompletions(this, null);
        }
    }

    private void CheckFinalPlacementsOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (!isCompleted && IsFinalPuzzleMatching())
        {
            isCompleted = true;
            oceanArtifact.SetCanRotate(false);

            AudioManager.Play("Puzzle Complete");
            oceanArtifact.FlickerAllOnce();

            UIArtifactWorldMap.SetAreaStatus(Area.Ocean, ArtifactWorldMapArea.AreaStatus.color);
            UIArtifactMenus._instance.OpenArtifactAndShow(2);
        }
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
                }
                else
                {
                    int tid = int.Parse(tids);
                    // UIArtifact.SetButtonComplete(tid, current.grid[x, y].islandId == tid);
                    UIArtifact.SetButtonComplete(artifactButton.islandId, artifactButton.islandId == tid);
                }
            }
        }
    }

    public void ClearTreesToJungle() // called in ShopDialogueManager
    {
        treesToJungle.SetActive(false);
        CameraShake.Shake(1, 2);
        AudioManager.Play("Slide Explosion");
    }

    public void SpawnFezziwigReward() {
        Collectible c = GetCollectible("Strawberry");
        if (!PlayerInventory.Contains(c)) {
            c.gameObject.SetActive(true);
            AudioManager.Play("Puzzle Complete");
        }
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
}
