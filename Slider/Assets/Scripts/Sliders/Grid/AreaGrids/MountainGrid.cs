using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainGrid : SGrid
{
    public int layerOffset; //the y offset of the top layer from the bottom (used to calculate top tile y position)

    [SerializeField] private MountainCaveWall mountainCaveWall;
    [SerializeField] private GemMachine gemMachine;

    public Meltable iceRails;

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

    public override void Init() {
        InitArea(Area.Mountain);
        base.Init();
    }

    protected override void Start()
    {
        base.Start();
        AudioManager.PlayMusic("Mountain");
        UIEffects.FadeFromBlack();
    }
    
    /*private void OnEnable()     C: Currently broken, will look into fixing later
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
    }*/

    public override void EnableStile(STile stile, bool shouldFlicker = true)
    {
        if(stile.islandId == 7)
            CheckTile7Spawn();
        base.EnableStile(stile, shouldFlicker);
    }

    private void CheckTile7Spawn()
    {
        Debug.Log("Checking t7");
        if(GetTileAt(0,0).islandId == 7 || GetTileAt(1,2).islandId == 8) //C: this only works with the "right" setup, need to antisoftlock fully
            return;
        SwapTiles(GetStile(7), GetStile(8));
    }


    #region Minecart Specs

    public void SetCrystalDelivered(bool value)
    {
        crystalDelivered = value;
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

    public static void FixIceRails(){
        Instance.iceRails.Fix();
    }
}