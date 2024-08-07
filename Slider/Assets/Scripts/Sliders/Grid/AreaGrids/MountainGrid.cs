using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainGrid : SGrid
{
    public int layerOffset; //the y offset of the top layer from the bottom (used to calculate top tile y position)

    [SerializeField] private GemMachine gemMachine;
    [SerializeField] private SpriteSwapper crystalSpriteSwapper;
    public Minecart minecart;

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


    public override void Init() 
    {
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

        SaveSystem.Current.SetBool("caveDoor", true);
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
                StartCoroutine(WaitThenCheckAnchorDrop(dropTile));
        }        
    }

    private IEnumerator WaitThenCheckAnchorDrop(STile dropTile)
    {
        yield return new WaitUntil(() => !dropTile.IsMoving());

        if(dropTile.y >= 2)
        {
            STile lower = SGrid.Current.GetGrid()[dropTile.x, dropTile.y - 2];
            if(!lower.isTileActive)  
            {
                MountainArtifact uiArtifact = (MountainArtifact) MountainArtifact.GetInstance();
                uiArtifact.AnchorSwap(dropTile, lower);
            }
        }
                
    }

    private void Update() {
        float playerY = Player._instance.transform.position.y;
        if (playerOnBottom && playerY > 63f) {
            playerOnBottom = false;
            AudioManager.SetGlobalParameter("MountainTemperature", 0f);
        }
        else if (!playerOnBottom && playerY < 63f) {
            playerOnBottom = true;
            AudioManager.SetGlobalParameter("MountainTemperature", 1);
        }

        if (playerY < housingOffset / 2)
        {
            AudioManager.SetGlobalParameter("MountainTemperature", 0.5f);
        }
    }

    public override void EnableStile(STile stile, bool shouldFlicker = true)
    {
        if(stile.islandId == 7 && !stile.isTileActive)
            SaveSystem.Current.SetBool("forceAutoMoveMountain", true);
        if(stile.islandId == 8 && !stile.isTileActive)
        {
            base.EnableStile(stile, shouldFlicker);
            CheckForMountainCompletion();
            return;
        }
        base.EnableStile(stile, shouldFlicker);
    }


    protected override void CheckForCompletionOnSetGrid()
    {
        CheckForMountainCompletion();
    }

    public void CheckForMountainCompletion() {
        if(CheckGrid.contains(GetGridString(), "31_48_76_52")) {
            SaveSystem.Current.SetBool("forceAutoMoveMountain", false);
            StartCoroutine(ShowButtonAndMapCompletions());
            SaveSystem.Current.SetBool("completedMountain", true);
            AchievementManager.SetAchievementStat("completedMountain", false, 1);
            if(minecart.NumPickups <= 2 && gemMachine.numGems >= 3)
            {
                AchievementManager.SetAchievementStat("mountainMinMinecart", true, 1);
            }
        }
    }


    #region Minecart Specs

    public void BurtMinecartAction()
    {
        switch(minecart.mcState)
        {
            case MinecartState.Crystal:
                minecart.UpdateState(MinecartState.Empty);
                SetCrystalDelivered();
                break;
            case MinecartState.Lava:
                SaveSystem.Current.SetBool("MountainBurtLava", true);
                SaveSystem.Current.SetBool("MountainBurtEmpty", false);
                break;
            case MinecartState.Empty:
                SaveSystem.Current.SetBool("MountainBurtEmpty", true);
                SaveSystem.Current.SetBool("MountainBurtLava", false);
                break;
        }
    }
    
    public void SetCrystalDeliveredTrue() => SetCrystalDelivered();

    public void SetCrystalDelivered(bool fromSave = false)
    {
        crystalDelivered = true;
        if(!fromSave)
            AudioManager.Play("Puzzle Complete");
        SaveSystem.Current.SetBool("MountainCrystalDelivered", true);
        crystalSpriteSwapper.TurnOn();
    }

    public void CheckCrystalDelivery(Condition c) => c.SetSpec(crystalDelivered);


    #endregion


    #region Save/Load
    

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        if(profile.GetBool("MountainCrystalDelivered"))
            SetCrystalDelivered(true);
    }

    #endregion
}