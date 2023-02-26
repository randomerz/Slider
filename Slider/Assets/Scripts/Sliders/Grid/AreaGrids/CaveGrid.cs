using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGrid : SGrid
{
    private bool allTilesLit = false;

    [SerializeField] private CaveDoor caveDoor;
    [SerializeField] private MountainCaveWall mountainCaveWall;
    [SerializeField] private CaveArtifactLightSim lightSim;

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
        }
    }

    private void OnDisable()
    {
        if (checkCompletion)
        {
            SGridAnimator.OnSTileMoveEnd -= checkCompletionsOnMoveFunc;
        }
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

    public void StartFinalPuzzle() 
    {
        SGridAnimator.OnSTileMoveEnd += checkCompletionsOnMoveFunc;

        checkCompletion = true;
        SaveSystem.Current.SetBool("cavesCompletion", checkCompletion);
        SaveSystem.Current.SetBool("forceAutoMove", true);

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
                for (int y = 0; y < Current.Width; y++)
                {
                    if (grid[x, y].isTileActive)
                    {
                        bool currLit = (grid[x, y] as CaveSTile).GetTileLit();
                        if (!currLit)
                        {
                            allTilesLit = false;
                        }
                        // UIArtifact.SetButtonComplete(grid[x, y].islandId, currLit);
                    }
                }
            }
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

        int[,] completedPuzzle = new int[3, 3] { { 9, 1, 5 },
                                                 { 6, 3, 4 },
                                                 { 8, 7, 2 } };
        SetGrid(completedPuzzle);
        lightSim.UpdateLightSim();
        StartCoroutine(CheckCompletionsAfterDelay(1.1f));
        SaveSystem.Current.SetBool("forceAutoMove", false);

        UIArtifactWorldMap.SetAreaStatus(Area.Caves, ArtifactWorldMapArea.AreaStatus.color);
        UIArtifactMenus._instance.OpenArtifactAndShow(2, true);
    }
}
