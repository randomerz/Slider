using UnityEngine;

public class OceanGrid : SGrid
{
    // public static OceanGrid instance;

    private static bool checkCompletion = false; // TODO: serialize all these
    private static bool isCompleted = false;
    public static bool canRotate = true; // global variable, used in OceanArtifact

    public GameObject burriedGuyNPC;
    public KnotBox knotBox;
    public LostGuyMovement lostGuyMovement;
    public OceanArtifact oceanArtifact; // used for the final quest to lock movement
    public GameObject treesToJungle;

    private Vector2Int[] correctPath =
    {
        Vector2Int.left,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.left,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.left,
    };
    private int playerIndex = 0;
    private Vector2Int playerMovement;
    private int lastIslandId = 1;
    public GameObject fog6;
    public GameObject fog7;
    public GameObject fogIsland;

    private new void Awake() {
        myArea = Area.Ocean;

        foreach (Collectible c in collectibles) // maybe don't have this
        {
            c.SetArea(myArea);
        }

        base.Awake();


        // instance = this;

        // StartCoroutine(test());
    }


    void Start()
    {
        foreach (Collectible c in collectibles)
        {
            if (PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(false);
            }

        }

        burriedGuyNPC.SetActive(false);

        AudioManager.PlayMusic("Ocean");
        UIEffects.FadeFromBlack();

    }

    private void OnEnable()
    {
        if (checkCompletion) {
            SGrid.OnGridMove += SGrid.UpdateButtonCompletions;
        }

        SGridAnimator.OnSTileMoveEnd += CheckShipwreck;
        SGridAnimator.OnSTileMoveEnd += CheckVolcano;
    }

    private void OnDisable()
    {
        if (checkCompletion) {
            SGrid.OnGridMove -= SGrid.UpdateButtonCompletions;
        }

        SGridAnimator.OnSTileMoveEnd -= CheckShipwreck;
        SGridAnimator.OnSTileMoveEnd -= CheckVolcano;
    }

    private void Update()
    {

        //Get tile the player is on. if it change from last update, find the direction the player moved. Add direction to list of moves. put list in checkfoggy. only why on fog tiles
        updatePlayerMovement();

    }

    public override void SaveGrid()
    {
        base.SaveGrid();
    }

    public override void LoadGrid()
    {
        base.LoadGrid();
    }


    public override void EnableStile(STile stile, bool shouldFlicker=true)
    {
        base.EnableStile(stile, shouldFlicker);

        stile.GetComponentInChildren<SpriteMask>().enabled = false; // on STile/SlideableArea

        if (grid != null)
        {
            CheckShipwreck(this, null);
            CheckVolcano(this, null);
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
        return CheckGrid.contains(GetGridString(),".4._895_.3.");
    }



    public void ActivateBurriedNPC()
    {
        burriedGuyNPC.SetActive(true);
    }

    private bool BuoyConditions()
    {
        if (!(GetStile(1).isTileActive && GetStile(3).isTileActive && GetStile(4).isTileActive && GetStile(8).isTileActive && GetStile(9).isTileActive))
        {
            return false;
        }

        if (!knotBox.isActiveAndEnabled)
        {
            return false;
        }

        return knotBox.CheckLines();
    }

    public void BuoyAllFound(Conditionals.Condition c)
    {
        if (!(GetStile(1).isTileActive && GetStile(3).isTileActive && GetStile(4).isTileActive && GetStile(8).isTileActive && GetStile(9).isTileActive))
        {
            c.SetSpec(false);
        }
        else
        {
            c.SetSpec(true);
        }
    }

    public void knotBoxEnabled(Conditionals.Condition c)
    {
        if (!knotBox.isActiveAndEnabled && (GetStile(1).isTileActive && GetStile(3).isTileActive && GetStile(4).isTileActive && GetStile(8).isTileActive && GetStile(9).isTileActive))
        {
            c.SetSpec(true);
        }
        else
        {
            c.SetSpec(false);
        }
    }

    public void BuoyCheck(Conditionals.Condition c)
    {
        c.SetSpec(BuoyConditions());
    }

    public void ToggleKnotBox()
    {
        knotBox.enabled = !knotBox.enabled;
    }

    public void IsCompleted(Conditionals.Condition c)
    {
        c?.SetSpec(checkCompletion && IsFinalPuzzleMatching());
    }

    private bool IsFinalPuzzleMatching()
    {
        return CheckGrid.contains(GetGridString(), "412_[^1248]{2}8_[^1248]{3}");
    }

    public void IsLostGuyBeached(Conditionals.Condition c)
    {
        c.SetSpec(lostGuyMovement.hasBeached);
    }

    // Foggy Seas
    
    public void CheckFoggySeas(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        FoggyCorrectMovement();

        if (GetStile(6).isTileActive && GetStile(7).isTileActive && FoggyCompleted())
        {
            fogIsland.transform.position = Player.GetStileUnderneath().transform.position;
            fogIsland.SetActive(true);

            if (Player.GetStileUnderneath().islandId == 6)
            {
                fog6.SetActive(false);
                
            }
            else 
            {
                fog7.SetActive(false);
            }
        }
    }

    private void updatePlayerMovement()
    {

        if (Player.GetStileUnderneath() == null)
        {
            return;
        }

        int currentIslandId = Player.GetStileUnderneath().islandId;
        
        if (currentIslandId != lastIslandId)
        {
            fog7.SetActive(true);
            fog6.SetActive(true);
            fogIsland.SetActive(false);

            if (currentIslandId != 6 && currentIslandId != 7)
            {
                failFoggy();
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
    }

    public void FoggyCorrectMovement()
    {
        if(correctPath[playerIndex] == playerMovement)
        {
            playerIndex++;
            FoggySeasAudio();
        }
        else 
        {
            failFoggy();
        }

    }

    private void failFoggy()
    {
        if (playerIndex != 0)
        {
            AudioManager.Play("Artifact Error");
        }
        playerIndex = 0;
        
        
    }

    public bool FoggyCompleted()
    {
        return playerIndex == 9;
    }

    // Final puzzle
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
        }
    }

    protected override void UpdateButtonCompletionsHelper()
    {
        for (int x = 0; x < current.width; x++) {
            for (int y = 0; y < current.width; y++) {
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
                else {
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
}
