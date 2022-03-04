using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGrid : SGrid
{
    public static CaveGrid instance;

    private bool[,] lightMap;

    public class OnLightMapUpdateArgs
    {
        public bool[,] lightMap;
    }
    public static event System.EventHandler<OnLightMapUpdateArgs> OnLightMapUpdate;

    private bool checkLightingCompletion = false;

    private bool allTilesLit = false;

    static System.EventHandler<SGridAnimator.OnTileMoveArgs> checkCompletionsOnMoveFunc;



    private new void Awake() {
        myArea = Area.Caves;

        foreach (Collectible c in collectibles)
        {
            c.SetArea(myArea);
        }

        base.Awake();

        instance = this;

        lightMap = new bool[3, 3] { { false, false, true},
                                    { false, false, false},
                                    { false, false, true},
                                  };

        checkCompletionsOnMoveFunc = (sender, e) => { CheckLightingCompletions(); };
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

        GetCollectible("Slider 5").gameObject.SetActive(false); // gameboy puzzle
        GetCollectible("Slider 6").gameObject.SetActive(false); // flashlight puzzle
        GetCollectible("Slider 9").gameObject.SetActive(false); // final puzzle

        AudioManager.PlayMusic("Connection");
        UIEffects.FadeFromBlack();

        SGrid.OnGridMove += (sender, e) => { Debug.Log(GetGridString()); };
    }

    private void OnEnable()
    {
        if (checkLightingCompletion)
        {
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

    public bool GetLit(int x, int y)
    {
        return lightMap[x, y];
    }

    public void SetLit(int x, int y, bool value)
    {
        lightMap[x, y] = value;

        OnLightMapUpdate?.Invoke(this, new OnLightMapUpdateArgs { lightMap = this.lightMap });
    }

    public void StartFinalPuzzle() {
        SGridAnimator.OnSTileMoveEnd += checkCompletionsOnMoveFunc;
        checkLightingCompletion = true;
        CheckLightingCompletions();
    }

    private void CheckLightingCompletions()
    {
        allTilesLit = true;
        for (int x = 0; x < current.width; x++)
        {
            for (int y = 0; y < current.width; y++)
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

    public void SetCavesCompleteCondition(Conditionals.Condition c)
    {
        c.SetSpec(checkLightingCompletion && allTilesLit);
    }

    // Puzzle 8 - light  up caves
    public void FinishCaves()
    {
        GivePlayerTheCollectible("Slider 9");

        // fading stuff
        UIEffects.FlashWhite();
        CameraShake.Shake(1.5f, 1.0f);


        int[,] completedPuzzle = new int[3, 3] { { 2, 1, 5 },
                                                 { 6, 3, 4 },
                                                 { 8, 7, 9 } };
        SetGrid(completedPuzzle);
        SGrid.CheckCompletions(this, null);
    }

    public override void SaveGrid() 
    {
        base.SaveGrid();
    }

    public override void LoadGrid()
    {
        base.LoadGrid();
    }
}
