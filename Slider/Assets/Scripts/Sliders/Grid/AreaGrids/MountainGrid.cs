using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainGrid : SGrid
{
    public int layerOffset; //the y offset of the top layer from the bottom (used to calculate top tile y position)

    [SerializeField] private MountainCaveWall mountainCaveWall;
    [SerializeField] private GemMachine gemMachine;

    private bool playerOnBottom = true;

    public static MountainGrid Instance => SGrid.Current as MountainGrid;

    /* C: The mountian sgrid is a 2 by 4 grid. The top 4 tiles represent the top layer,
        while the bottom 4 tiles represent the bottom layer. For example, the following grid

                5 1
                2 8

                4 7
    (0,0) ->    6 3

        represents the grid with 5, 1, 2, and 8 on the top layer.
    */


    private bool crystalDelivered = false;
    private float musicValue = 0;

    private Coroutine musicTransitionCoroutine;

    public override void Init() {
        InitArea(Area.Mountain);
        base.Init();
    }

    protected override void Start()
    {
        base.Start();
        playerOnBottom = Player._instance.transform.position.y < 63f;
        musicValue = playerOnBottom ? 1 : 0;
        AudioManager.SetMusicParameter("Mountain", "MountainTemperature", musicValue);
        AudioManager.PlayMusic("Mountain");
    }
    
    private void OnEnable()     
    {
        Anchor.OnAnchorInteract += OnAnchorInteract;
    }

    private void OnDisable()
    {
        Anchor.OnAnchorInteract -= OnAnchorInteract;
    }

    private void OnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs interactArgs)
    {
        if (interactArgs.drop)
        {
            STile dropTile = interactArgs.stile;
            if(dropTile != null)
            {
                if(dropTile.y < 2)
                    return; //currently using the anchor on the bottom layer does nothing
                STile lower = SGrid.Current.GetGrid()[dropTile.x, dropTile.y - 2];
                if(!lower.isTileActive)  //if this is true, then there is not an active tile below the current tile
                {
                    //C TODO: look at how logan did conveyers and copy that because rn this cancels the whole queue
                    MountainArtifact uiArtifact = (MountainArtifact) MountainArtifact.GetInstance();
                    UIArtifact.ClearQueues();
                    uiArtifact.AnchorSwap(dropTile, lower);
                }
            }
        }        
    }

    private void Update() {
        if(playerOnBottom && Player._instance.transform.position.y > 63f) {
            playerOnBottom = false;
            if(musicTransitionCoroutine != null)
                StopCoroutine(musicTransitionCoroutine);
            musicTransitionCoroutine = StartCoroutine(TransitionMusic(musicValue, 0, 2));
        }
        if(!playerOnBottom && Player._instance.transform.position.y < 63f) {
            playerOnBottom = true;
            if(musicTransitionCoroutine != null)
                StopCoroutine(musicTransitionCoroutine);
            musicTransitionCoroutine = StartCoroutine(TransitionMusic(musicValue, 1, 2));
        }
    }

    private IEnumerator TransitionMusic(float start, float end, float duration)
    {
        float t = 0;
        while(t < duration) {
            t += Time.deltaTime;
            musicValue = Mathf.Lerp(start, end, t/duration);
            AudioManager.SetMusicParameter("Mountain", "MountainTemperature", musicValue);
            yield return null;
        }
        musicValue = end;
        AudioManager.SetMusicParameter("Mountain", "MountainTemperature", end);
    }

    public override void EnableStile(STile stile, bool shouldFlicker = true)
    {
        if(stile.islandId == 7)
            CheckTile7Spawn();
        base.EnableStile(stile, shouldFlicker);
    }

    private void CheckTile7Spawn()
    {
        //UPDATE TO FORCE PARITY
        int[,] t7exact = new int[,]{{7,1,5,3},{2,6,8,4}};
        if(!CheckGrid.contains(GetGridString(true), "34_58_16_72" )) 
        {
            if(!CheckGrid.contains(GetGridString(true), "34_57_16_82" ))
            {
                Minecart mc = FindObjectOfType<Minecart>();
                mc?.StopMoving();
            }
            UIArtifact.ClearQueues();
            SetGrid(t7exact);
        }
    }

    public void FinishMountain()
    {
        EnableStile(8);

        int[,] completedPuzzle = new int[2, 4] { {4, 7, 5, 3},
                                                 {2, 6, 8, 1}};
        SetGrid(completedPuzzle);
        SaveSystem.Current.SetBool("forceAutoMove", false);

        UIArtifactWorldMap.SetAreaStatus(Area.Mountain, ArtifactWorldMapArea.AreaStatus.color);
        UIArtifactMenus._instance.OpenArtifactAndShow(2, true);
    }


    #region Minecart Specs

    public void SetCrystalDelivered(bool value)
    {
        crystalDelivered = value;
        AudioManager.Play("Puzzle Complete");
        SaveSystem.Current.SetBool("MountainCrystalDelivered", crystalDelivered);
    }

    public void CheckCrystalDelivery(Condition c)
    {
        c.SetSpec(crystalDelivered);
    }


    #endregion


    #region Save/Load

    //C: for some reason the meltables save on their own but don't load
    public override void Save()
    {
        base.Save();
        mountainCaveWall.Save();
        gemMachine.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        mountainCaveWall.Load(profile);
        gemMachine.Load(profile);
        crystalDelivered = profile.GetBool("MountainCrystalDelivered", crystalDelivered);
        Meltable[] meltables = FindObjectsOfType<Meltable>();
        foreach(Meltable m in meltables)
            m.Load(profile);
    }

    #endregion
}