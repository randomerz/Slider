using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGrid : SGrid
{
    private static bool checkLightingCompletion = false;

    private bool allTilesLit = false;

    [SerializeField] private CaveDoor caveDoor;
    [SerializeField] private MountainCaveWall mountainCaveWall;

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
        UIEffects.FadeFromBlack();
    }

    private void OnEnable()
    {
        if (checkLightingCompletion)
        {
            checkCompletionsOnMoveFunc(this, null);
            SGridAnimator.OnSTileMoveEnd += checkCompletionsOnMoveFunc;
        }
    }

    private void OnDisable()
    {
        if (checkLightingCompletion)
        {
            SGridAnimator.OnSTileMoveEnd -= checkCompletionsOnMoveFunc;
        }
    }

    public void StartFinalPuzzle() {
        SGridAnimator.OnSTileMoveEnd += checkCompletionsOnMoveFunc;
        checkLightingCompletion = true;
        CheckLightingCompletions();
    }

    private void CheckLightingCompletions()
    {
        //L: Scuffy me Luffy
        if (SGrid.Current != null && (SGrid.Current as CaveGrid) != null)
        {
            allTilesLit = true;
            for (int x = 0; x < Current.Width; x++)
            {
                for (int y = 0; y < Current.Width; y++)
                {
                    if (grid[x, y].isTileActive)
                    {
                        bool currLit = (grid[x, y] as CaveSTile).GetTileLit();
                        if (!currLit)
                        {
                            allTilesLit = false;
                        }
                        UIArtifact.SetButtonComplete(grid[x, y].islandId, currLit);
                    }
                }
            }
        }
    }

    public void SetCavesCompleteCondition(Condition c)
    {
        c.SetSpec(checkLightingCompletion && allTilesLit);
    }

    public void CavesShake1()
    {
        CameraShake.Shake(0.25f, 0.25f);
        AudioManager.Play("Slide Rumble");
    }
    public void CavesShake2()
    {
        CameraShake.Shake(0.75f, 0.5f);
        AudioManager.Play("Slide Rumble");
    }

    public void CavesShake3()
    {
        CameraShake.Shake(1.5f, 2.5f);
        AudioManager.Play("Slide Explosion");
        UIEffects.FlashWhite();
    }

    // Puzzle 8 - light  up caves
    public void FinishCaves()
    {
        GivePlayerTheCollectible("Slider 9");

        int[,] completedPuzzle = new int[3, 3] { { 2, 1, 5 },
                                                 { 6, 3, 4 },
                                                 { 8, 7, 9 } };
        SetGrid(completedPuzzle);
        StartCoroutine(CheckCompletionsAfterDelay(1.1f));

        UIArtifactWorldMap.SetAreaStatus(Area.Caves, ArtifactWorldMapArea.AreaStatus.color);
    }

    public override void Save() 
    {
        base.Save();
        // caveDoor.Save();
      //  mountainCaveWall.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        // caveDoor.Load(profile);
        mountainCaveWall.Load(profile);
    }
}
