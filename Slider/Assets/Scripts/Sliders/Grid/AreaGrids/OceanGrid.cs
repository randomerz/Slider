using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class OceanGrid : SGrid
{
    public GameObject burriedGuyNPC;
    public GameObject burriedTreasure;
    public ParticleSystem burriedTreasureParticles;
    public NPCRotation npcRotation;
    public OceanArtifact oceanArtifact; 
    public GameObject treesToJungle;
    private List<int> landTiles = new List<int> {1, 2, 4, 8};    
    private int numAnchorUses;
    [SerializeField] private Volcano volcano;

    private bool isCompleted = false;



    public override void Init()
    {
        InitArea(Area.Ocean);
        base.Init();
    }


    protected override void Start()
    {
        base.Start();
        burriedGuyNPC.SetActive(false);

        AudioManager.PlayMusic("Ocean");
        AudioManager.PlayMusic("Ocean Tavern", false); // for FMOD effects
        AudioManager.PlayMusic("Ocean uwu", false); // for FMOD effects
    }

    private void OnEnable()
    {
        if (checkCompletion && !isCompleted)
        {
            UpdateButtonCompletions(this, null);
            SubscribeCompletionEvents();
        }

        SGridAnimator.OnSTileMoveEnd += CheckShipwreck;
        UIArtifactMenus.OnArtifactOpened += CheckShipwreck;
        SGridAnimator.OnSTileMoveEnd += CheckVolcano;
        UIArtifactMenus.OnArtifactOpened += CheckVolcano;
        Anchor.OnAnchorInteract += OnAnchorInteract;
    }

    private void SubscribeCompletionEvents()
    {
        OnGridMove += UpdateButtonCompletions; // this is probably not needed
        UIArtifact.OnButtonInteract += SGrid.UpdateButtonCompletions;
        SGridAnimator.OnSTileMoveEnd += CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletion
        UIArtifactMenus.OnArtifactOpened += CheckFinalPlacementsOnMove;
    }

    private void OnDisable()
    {
        if (checkCompletion && !isCompleted)
        {
            UnsubscribeCompletionEvents();
        }

        SGridAnimator.OnSTileMoveEnd -= CheckShipwreck;
        UIArtifactMenus.OnArtifactOpened -= CheckShipwreck;
        SGridAnimator.OnSTileMoveEnd -= CheckVolcano;
        UIArtifactMenus.OnArtifactOpened -= CheckVolcano;
        Anchor.OnAnchorInteract -= OnAnchorInteract;
    }

    private void UnsubscribeCompletionEvents()
    {
        OnGridMove -= UpdateButtonCompletions; // this is probably not needed
        UIArtifact.OnButtonInteract -= SGrid.UpdateButtonCompletions;
        SGridAnimator.OnSTileMoveEnd -= CheckFinalPlacementsOnMove;// SGrid.OnGridMove += SGrid.CheckCompletions
        UIArtifactMenus.OnArtifactOpened -= CheckFinalPlacementsOnMove;
    }
    
    private void OnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs e)
    {
        if(!e.fromStart && e.drop)
            numAnchorUses++;
    }

    public override void Save()
    {
        base.Save();
        SaveSystem.Current.SetBool("oceanUnlockedAllSliders", npcRotation.unlockedAllSliders);
        SaveSystem.Current.SetBool("oceanBreadgeCollected", npcRotation.gotBreadge);
        SaveSystem.Current.SetInt("oceanAnchorUses", numAnchorUses);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        checkCompletion = profile.GetBool("oceanCompletion");
        isCompleted = profile.GetBool("oceanCompleted");
        if (isCompleted) ((OceanArtifact)OceanArtifact._instance).SetCanRotate(false);

        treesToJungle.SetActive(!profile.GetBool("oceanTreesRemoved"));

        npcRotation.unlockedAllSliders = profile.GetBool("oceanUnlockedAllSliders");
        npcRotation.gotBreadge = profile.GetBool("oceanBreadgeCollected");
        numAnchorUses = profile.GetInt("oceanAnchorUses");
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
                if (TrySwapTile2(stile, stile1x + 1, stile1y + 1))
                    return;
            }
            else if (stile1y > 0)
            {
                if (TrySwapTile2(stile, stile1x + 1, stile1y - 1))
                    return;
            }
        }
        else if (stile1x > 0)
        {
            if (stile1y < 2)
            {
                if (TrySwapTile2(stile, stile1x - 1, stile1y + 1))
                    return;
            }
            else if (stile1y > 0)
            {
                if (TrySwapTile2(stile, stile1x - 1, stile1y - 1))
                    return;
            }
        }

        // wah wah u couldnt swap it for some reason boohoo
    }

    private bool TrySwapTile2(STile slider2, int x, int y)
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
        if (IsShipwreckAdjacent() && !PlayerInventory.Contains("Treasure Chest"))
        {
            burriedTreasure.SetActive(true);
            burriedTreasureParticles.Play();
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
            SubscribeCompletionEvents();
            SGrid.UpdateButtonCompletions(this, null);
        }
    }
    
    private void CheckFinalPlacementsOnMove(object sender, System.EventArgs e)
    {
        if (!isCompleted && IsFinalPuzzleMatching())
        {
            isCompleted = true;
            UnsubscribeCompletionEvents();
            SaveSystem.Current.SetBool("oceanCompleted", checkCompletion);
            ((OceanArtifact)OceanArtifact._instance).SetCanRotate(false);

            AudioManager.Play("Puzzle Complete");
            oceanArtifact.FlickerAllOnce();

            UIArtifactWorldMap.SetAreaStatus(Area.Ocean, ArtifactWorldMapArea.AreaStatus.color);

            StartCoroutine(ShowMapAfterDelay(1));


            AchievementManager.SetAchievementStat("completedOcean", 1);
            if (numAnchorUses <= 2) {
                //Give 2 use Anchor Achievement 
                AchievementManager.SetAchievementStat("completedOceanMinAnchor", 1);
            }
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
                char tids = GetTileIdAt(x, y);
                ArtifactTileButton artifactButton = UIArtifact.GetButton(x, y);
                if (tids == '*')
                {
                    int abid = artifactButton.islandId;
                    bool isLand = landTiles.Contains(abid);
                    UIArtifact.SetButtonComplete(artifactButton.islandId, !isLand);
                    UIArtifact.GetButton(artifactButton.x, artifactButton.y).SetHighlighted(isLand);
                }
                else
                {
                    int tid = Converter.CharToInt(tids);
                    UIArtifact.SetButtonComplete(artifactButton.islandId, artifactButton.islandId == tid);
                    UIArtifact.GetButton(artifactButton.x, artifactButton.y).SetHighlighted(artifactButton.islandId != tid);
                }
            }
        }
    }

    public void ClearTreesToJungle()
    {
        SaveSystem.Current.SetBool("oceanTreesRemoved", true);
        treesToJungle.SetActive(false);
        CameraShake.Shake(1, 2);
        AudioManager.Play("Slide Explosion");
    }
}
