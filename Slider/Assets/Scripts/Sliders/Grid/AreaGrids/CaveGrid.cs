using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGrid : SGrid
{
    private bool allTilesLit = false;

    [SerializeField] private List<Animator> largeMagicRocksAnimators;
    [SerializeField] private GameObject magicRocksIcon;
    [SerializeField] private CaveDoor caveDoor;
    [SerializeField] private MountainCaveWall mountainCaveWall;
    [SerializeField] private CaveArtifactLightSim lightSim;

    private Coroutine magicRocksIconFlashCoroutine;
    [SerializeField] private GameObject cavesMagicParticles;
    private List<GameObject> particles = new List<GameObject>();

    static System.EventHandler<SGridAnimator.OnTileMoveArgs> checkCompletionsOnMoveFunc;

    public override void Init() {
        InitArea(Area.Caves);
        base.Init();

        checkCompletionsOnMoveFunc = (sender, e) => { CheckLightingCompletions(); };
        
    }


    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("Caves");
    }

    private void OnEnable()
    {
        if (checkCompletion)
        {
            checkCompletionsOnMoveFunc.Invoke(this, null);
            SGridAnimator.OnSTileMoveEnd += checkCompletionsOnMoveFunc;
        } else
        {
            SGridAnimator.OnSTileMoveEnd -= checkCompletionsOnMoveFunc;
        }
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveEnd -= checkCompletionsOnMoveFunc;
    }

    public override void Save()
    {
        base.Save();

        SaveSystem.Current.SetBool("cavesCompletion", checkCompletion);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);

        checkCompletion = profile.GetBool("cavesCompletion");

        if (checkCompletion)
        {
            gridAnimator.ChangeMovementDuration(0.5f);
        }

        mountainCaveWall.Load(profile); // needed?
    }

    public override void EnableStile(STile stile, bool shouldFlicker = true)
    {
        base.EnableStile(stile, shouldFlicker);
        if(stile.islandId == 7)
        {
            AchievementManager.SetAchievementStat("completedBigMoss", 1);
        }
    }

    public void StartFinalPuzzle() 
    {
        SGridAnimator.OnSTileMoveEnd += checkCompletionsOnMoveFunc;

        if (!checkCompletion)
        {
            AudioManager.Play("Puzzle Complete");
        }
        checkCompletion = true;

        gridAnimator.ChangeMovementDuration(0.5f);

        SaveSystem.Current.SetBool("cavesCompletion", checkCompletion);
        SaveSystem.Current.SetBool("forceAutoMoveCaves", true);

        CheckLightingCompletions();
        lightSim.UpdateLightSim();
    }

    private void CheckLightingCompletions()
    {
        //L: Scuffy me Luffy
        if (SGrid.Current != null && (SGrid.Current as CaveGrid) != null)
        {
            allTilesLit = true;
            for (int x = 0; x < Current.Width; x++)
            {
                for (int y = 0; y < Current.Height; y++)
                {
                    if (grid[x, y].isTileActive)
                    {
                        CaveSTile stile = (grid[x, y] as CaveSTile);
                        if (stile == null)
                        {
                            Debug.LogError("Found a stile that was null for some reason!");
                            return;
                        }
                        
                        bool currLit = stile.GetTileLit();
                        if (!currLit)
                        {
                            allTilesLit = false;
                        }
                        // UIArtifact.SetButtonComplete(grid[x, y].islandId, currLit);
                    }
                }
            }
            SetMagicRocks(allTilesLit);
        }
    }

    public void SetCavesCompleteCondition(Condition c)
    {
        c.SetSpec(checkCompletion && allTilesLit);
    }

    // These are for NPC dialogue to call
    public void CavesCloseArtifact()
    {
        UIArtifactMenus._instance.CloseArtifact();
    }

    public void CavesShake1()
    {
        CameraShake.Shake(0.25f, 0.25f);
        AudioManager.Play("Slide Rumble");

        StartCoroutine(CaveMagicParticleAnimation(GetStile(8).transform.position, 4));

        AudioManager.DampenMusic(this, 0, 9);
        AudioManager.Play("Caves Win");
    }

    public void CavesShake2()
    {
        CameraShake.Shake(0.75f, 0.5f);
        AudioManager.Play("Slide Rumble");
    }

    public void CavesShake3()
    {
        StartCoroutine(ICavesShake3());
    }

    private IEnumerator ICavesShake3()
    {
        yield return new WaitForSeconds(0.5f);

        CameraShake.Shake(1.5f, 2.5f);
        AudioManager.Play("Slide Explosion");
        UIEffects.FlashWhite();

        yield return new WaitForSeconds(1.0f);

        FinishCaves();
    }

    // Puzzle 8 - light  up caves
    public void FinishCaves()
    {
        int[,] completedPuzzle = new int[3, 3] { { 9, 1, 5 },
                                                 { 6, 3, 4 },
                                                 { 8, 7, 2 } };
        SetGrid(completedPuzzle);

        GivePlayerTheCollectible("Slider 9");

        StartCoroutine(CheckCompletionsAfterDelay(1.1f));
        SaveSystem.Current.SetBool("forceAutoMoveCaves", false);

        lightSim.UpdateLightSim();
        SetMagicRocks(false);

        UIArtifactWorldMap.SetAreaStatus(Area.Caves, ArtifactWorldMapArea.AreaStatus.color);
        UIArtifactMenus._instance.OpenArtifactAndShow(2, true);

        AchievementManager.SetAchievementStat("completedCaves", 1);
    }

    private void SetMagicRocks(bool value)
    {
        if (!value && magicRocksIconFlashCoroutine != null)
        {
            StopCoroutine(magicRocksIconFlashCoroutine);
            magicRocksIconFlashCoroutine = null;
            magicRocksIcon.SetActive(false);
        }
        else if (value)
        {
            if (magicRocksIconFlashCoroutine == null)
            {
                AudioManager.Play("Puzzle Complete");
                magicRocksIconFlashCoroutine = StartCoroutine(AnimateMagicRockIcon());
            }
        }

        foreach (Animator a in largeMagicRocksAnimators)
        {
            a.SetBool("isMagic", value);
        }
    }

    private IEnumerator AnimateMagicRockIcon()
    {
        while (true)
        {
            magicRocksIcon.SetActive(true);

            yield return new WaitForSeconds(0.5f);
            
            magicRocksIcon.SetActive(false);

            yield return new WaitForSeconds(0.5f);
        }
    }

    // this could be optimized a lot
    private IEnumerator CaveMagicParticleAnimation(Vector3 position, int numRecur)
    {
        if (numRecur == 0)
            yield break;

        for (int i = 0; i < 4; i++)
        {
            particles.Add(GameObject.Instantiate(cavesMagicParticles, position + GetRandomPosition(), Quaternion.identity, stiles[7].transform)); // stile 8 transform

            yield return new WaitForSeconds(0.25f);
        }

        StartCoroutine(CaveMagicParticleAnimation(position, numRecur - 1));

        for (int i = 0; i < 32; i++)
        {
            particles.Add(GameObject.Instantiate(cavesMagicParticles, position + GetRandomPosition(), Quaternion.identity, stiles[7].transform));

            yield return new WaitForSeconds(0.25f);
        }
    }
    
    private Vector3 GetRandomPosition()
    {
        float r = Random.Range(0f, 8f);
        float t = Random.Range(0f, 360f);

        return new Vector2(r * Mathf.Cos(t), r * Mathf.Sin(t));
    }
}
