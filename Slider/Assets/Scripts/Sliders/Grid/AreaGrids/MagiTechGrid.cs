using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MagiTechGrid : SGrid
{
    
    public static MagiTechGrid Instance => SGrid.Current as MagiTechGrid;

    public int gridOffset = 100; //C: The X distance between the present and past grid

     /*C: Note that this is on the *opposite* side of the grid from the anchor.
    *   IE if the anchor is dropped at (2,1), in the present, this vector will be (5, 1),
    *   the corresponding location in the past, since this is the location we need
    *   to compare against
    */
    public Vector2Int desyncLocation = new Vector2Int(-1, -1);

    //C: likewise this is the ID of the *opposite* Stile
    public int desyncIslandId = -1;

    //Location of the anchored tile
    public Vector2Int desyncAnchoredTileLocation = new Vector2Int(-1, -1);

    //ID of anchored tile
    public int desyncAnchoredIslandId = -1;

    //True when the opposite tile is at a different location
    public bool DesyncActive = false;

    public class OnDesyncArgs : EventArgs
    {
        public int desyncIslandId;
        public Vector2Int desyncLocation;
        public int anchoredTileIslandId;
        public Vector2Int anchoredTileLocation;

        public OnDesyncArgs(int desyncIslandId, Vector2Int desyncLocation, int anchoredTileIslandId, Vector2Int anchoredTileLocation)
        {
            this.desyncIslandId = desyncIslandId;
            this.desyncLocation = desyncLocation;
            this.anchoredTileIslandId = anchoredTileIslandId;
            this.anchoredTileLocation = anchoredTileLocation;
        }
    }

    public static EventHandler<OnDesyncArgs> OnDesyncStartWorld;
    public static EventHandler<OnDesyncArgs> OnDesyncEndWorld;

    [SerializeField] private Collider2D fireStoolZoneCollider;
    [SerializeField] private Collider2D lightningStoolZoneCollider;

    private int numOil = 0;

    [SerializeField] private MagiTechTabManager tabManager;
    [SerializeField] private PlayerActionHints hints;

    

    private ContactFilter2D contactFilter;

    public List<GameObject> BridgeObjects;

    /* C: The Magitech grid is a 6 by 3 grid. The left 9 STiles represent the present,
    and the right 9 STiles represent the past. The past tile will have an islandID
    exactly 9 more than its corresponding present tile. Note that in strings, the past tiles
    will be reprsented with the characters A-I so they can retain a length of 1.

    A Magitech grid might look like this

    1 2 3   A B C
    4 5 6   D E F
    7 8 9   G H I

    */


    //Intialization

    public override void Init()
    {
        InitArea(Area.MagiTech);
        base.Init();
    }

    protected override void Start()
    {
        base.Start();
        contactFilter = new ContactFilter2D();
        
        AudioManager.PlayMusic("MagiTech");
        AudioManager.SetMusicParameter("MagiTech", "MagiTechIsFuture", IsInPast(Player._instance.transform) ? 0 : 1);
    }

    protected void OnEnable()
    {
        Portal.OnTimeChange += OnTimeChange;
        Anchor.OnAnchorInteract += OnAnchorInteract;
        SGridAnimator.OnSTileMoveStart += OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
    }

    protected void OnDisable()
    {
        Portal.OnTimeChange -= OnTimeChange;
        Anchor.OnAnchorInteract -= OnAnchorInteract;
        SGridAnimator.OnSTileMoveStart -= OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
    }

    private void OnTimeChange(object sender, Portal.OnTimeChangeArgs e)
    {
        AudioManager.SetMusicParameter("MagiTech", "MagiTechIsFuture", e.fromPast ? 1 : 0);
    }

    #region Magitech Mechanics 

    public override void CollectSTile(int islandId)
    {
        foreach (STile s in grid)
        {
            if (s.islandId == islandId || s.islandId - 9 == islandId)
            {
                CollectStile(s);
            }
            if(s.islandId == 1)
            {
                tabManager.EnableTab();
            }
        }
    }

    public override void EnableStile(STile stile, bool shouldFlicker = true)
    {
        //All enabling logic handled by present tile
        if(stile.islandId > 9) return;

        if(ShouldCheckDesyncTilePlacement(stile))
            CheckDesyncTilePlacement(stile);

        base.EnableStile(stile, shouldFlicker);
        STile altTile = GetStile(stile.islandId + 9);
        base.EnableStile(altTile, shouldFlicker);
    }

    private bool ShouldCheckDesyncTilePlacement(STile tile)
    {
        return DesyncActive && !tile.isTileActive;
    }

    private void CheckDesyncTilePlacement(STile presentTile)
    {
        if(presentTile.islandId == 9) //if spawning the 9th tile and desync is active, then we have to end the desync for the tile to spawn correctly
        {
            EndDesync();
            return;
        }

        STile pastTile = FindAltStile(presentTile);
        if(TilesAligned(presentTile, pastTile))
            return;
        
        //check if either tile has an empty space in other dimension before resorting to swapping both tiles
        if(IsTileFreeInBothDimenions(presentTile))
        {
            STile swap = FindTileAtSameCoordsInOtherDimenions(presentTile);
            SwapTiles(swap, pastTile);
            return;
        }
        if(IsTileFreeInBothDimenions(pastTile))
        {
            STile swap = FindTileAtSameCoordsInOtherDimenions(pastTile);
            SwapTiles(swap, presentTile);
            return;
        }

        Vector2Int firstFreeSpace = FindLocationFreeInBothDimensions();
        if(firstFreeSpace.x == -1)
        {
            Debug.LogError("No grid space free in both dimensions");
            return;
        }

        STile presentSwap = GetStileAt(firstFreeSpace);
        STile pastSwap = GetStileAt(FindAltCoords(firstFreeSpace));
        SwapTiles(presentTile, presentSwap);
        SwapTiles(pastTile, pastSwap);
    }

    private STile FindAltStile(STile sTile)
    {
        int altId = FindAltId(sTile.islandId);
        return GetStile(altId);
    }

    private bool TilesAligned(STile tile1, STile tile2)
    {
        return tile1.x == ((tile2.x + 3) % 6) && tile1.y == tile2.y;
    }

    private STile FindTileAtSameCoordsInOtherDimenions(STile sTile)
    {
        Vector2Int altCoords = FindAltCoords(sTile.x, sTile.y);
        return GetStileAt(altCoords);
    }

    private bool IsTileFreeInBothDimenions(STile tile)
    {
        if(tile.isTileActive) return false;
        return !FindTileAtSameCoordsInOtherDimenions(tile).isTileActive;
    }

    private Vector2Int FindLocationFreeInBothDimensions()
    {
        for(int x = 0; x < 3; x++)
        {
            for(int y = 0; y < 3; y++)
            {
                if (!GetStileAt(x, y).isTileActive && !GetStileAt(FindAltCoords(x,y)).isTileActive)
                {
                    return new (x,y);
                }
            }
        }
        return new(-1, -1);
    }

    /// <summary>
    /// Magitech returns HALF of the total number of tiles.
    /// </summary>
    /// <returns></returns>
    public override int GetNumTilesCollected()
    {
        return base.GetNumTilesCollected() / 2;
    }

    public override int GetTotalNumTiles()
    {
        return Width * Height / 2;
    }

    public override bool AllButtonsComplete()
    {
       return GetNumButtonCompletions() == GetTotalNumTiles() * 2;
    }

    public override void Save()
    {
        if (desyncIslandId != -1)
        {
            EndDesync();
        }
        SaveSystem.Current.SetInt("MagitechOilCollected", numOil);
        base.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        if(GetNumTilesCollected() >= 1)
            tabManager.EnableTab();
        if(profile.GetBool("magitechBridgeFixed"))
            LowerDrawbridge(true);
        numOil = profile.GetInt("MagitechOilCollected");

    }

    public static bool IsInPast(Transform transform)
    {
        return transform.position.x > 67;
    }

    public void TryEnableHint()
    {
        if(GetNumTilesCollected() >= 1)
            hints.TriggerHint("altview");
    }

    private void OnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs interactArgs)
    {
        STile dropTile = interactArgs.stile;
        if (dropTile != null)
        {
            if (interactArgs.drop)
            {
                desyncLocation = FindAltCoords(dropTile.x, dropTile.y);
                desyncIslandId = FindAltId(dropTile.islandId);
                desyncAnchoredTileLocation = new(dropTile.x, dropTile.y);
                desyncAnchoredIslandId = dropTile.islandId;
            }
            else if (desyncIslandId != -1)
            {
                EndDesync();
            }
        }
    }

    private void EndDesync()
    {
        RestoreGridFromDesync();
        DesyncActive = false;
        OnDesyncEndWorld?.Invoke(this, new(desyncIslandId, desyncLocation, desyncAnchoredIslandId, desyncAnchoredTileLocation));
        desyncLocation = new Vector2Int(-1, -1);
        desyncIslandId = -1;
        desyncAnchoredTileLocation = new Vector2Int(-1, -1);
        desyncAnchoredIslandId = -1;
    }

    private void RestoreGridFromDesync()
    {
        STile[,] temp = Current.GetGrid();
        int[,] currGrid = new int[6, 3];
        int[,] newGrid = new int[6, 3];
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                currGrid[x, y] = temp[x, y].islandId;
                newGrid[x, y] = temp[x, y].islandId;
            }
        }

        int offset = (desyncLocation.x / 3) * 3;
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                newGrid[x + offset, y] = FindAltId(currGrid[x - offset + 3, y]);
            }
        }
        Current.SetGrid(newGrid);
    }

    private Vector2Int FindAltCoords(Vector2Int v)
    {
        return new Vector2Int((v.x + 3) % 6, v.y);
    }

    private Vector2Int FindAltCoords(int x, int y)
    {
        return new Vector2Int((x + 3) % 6, y);
    }

    private int FindAltId(int islandId)
    {        
        return (islandId == 9) ? 18 : (islandId + 9) % 18;
    }

    private void OnSTileMoveStart(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if(e.stile.islandId == desyncIslandId)
        {
            if(e.prevPos == desyncLocation) //moving away from "correct" location
            {
                DesyncActive = true;
                OnDesyncStartWorld?.Invoke(this, new(desyncIslandId, desyncLocation, desyncAnchoredIslandId, desyncAnchoredTileLocation));
            }
        }
    }

    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if(e.stile.islandId == desyncIslandId)
        {
            if(e.stile.x == desyncLocation.x && e.stile.y == desyncLocation.y) //back to "correct" location
            {
                DesyncActive = false;
                OnDesyncEndWorld?.Invoke(this, new(desyncIslandId, desyncLocation, desyncAnchoredIslandId, desyncAnchoredTileLocation));
            }
        }
    }

    public static bool IsTileDesynced(STile tile)
    {
        if(tile == null) return false;
        var m = Current as MagiTechGrid;
        return m.DesyncActive && (m.desyncIslandId == tile.islandId || m.desyncAnchoredIslandId == tile.islandId);
    }


    #endregion

    #region Misc methods

    public void DisableContractorBarrel()
    {
        // if (!contractorBarrel.activeSelf)
        // {
        //     contractorBarrel.SetActive(false);
        //     AudioManager.Play("Puzzle Complete");
        //     ParticleManager.SpawnParticle(ParticleType.SmokePoof, contractorBarrel.transform.position, contractorBarrel.transform.parent);
        // }
    }

    #endregion

    public void LowerDrawbridge(bool fromSave = false)
    {
        if(!fromSave)
        {
            SaveSystem.Current.SetBool("magitechBridgeFixed", true);
            AudioManager.Play("Puzzle Complete");
        }
        foreach(GameObject g in BridgeObjects)
        {
            g.SetActive(false);
        }
    }

    #region Conditions

    public void FireHasStool(Condition c)
    {
        // if (SaveSystem.Current.GetBool("magiTechFactory"))
        // {
        //     c.SetSpec(true);
        //     return;
        // }

        foreach (Collider2D hit in GetCollidingItems(fireStoolZoneCollider))
        {
            Item item = hit.GetComponent<Item>();
            if (item != null && (item.itemName == "Step Stool" || item.itemName == "Past Step Stool"))
            {
                c.SetSpec(true);
                return;
            }
        }
        
        c.SetSpec(false);
    }

    public void LightningHasStool(Condition c)
    {
        // if (SaveSystem.Current.GetBool("magiTechFactory"))
        // {
        //     c.SetSpec(true);
        //     return;
        // }

        foreach (Collider2D hit in GetCollidingItems(lightningStoolZoneCollider))
        {
            Item item = hit.GetComponent<Item>();
            if (item != null && (item.itemName == "Step Stool" || item.itemName == "Past Step Stool"))
            {
                c.SetSpec(true);
                return;
            }
        }

        c.SetSpec(false);
    }

    private List<Collider2D> GetCollidingItems(Collider2D collider)
    {
        List<Collider2D> list = new();
        collider.OverlapCollider(contactFilter, list);
        return list;
    }

    public void HasOneOil(Condition c)
    {
        c.SetSpec(numOil == 1);
    }

    public void HasTwoOil(Condition c)
    {
        c.SetSpec(numOil == 2);
    }
    public void HasThreeOil(Condition c)
    {
        c.SetSpec(numOil == 3);
    }

    public void IncrementOil()
    {
        numOil++;
    }

    public void IsDesyncActive(Condition c)
    {
        c.SetSpec(DesyncActive);
    }

    public void IsPlayerOnDesyncTile(Condition c)
    {
        c.SetSpec(DesyncActive && 
        Player.GetInstance().GetSTileUnderneath() != null &&
        (Player.GetInstance().GetSTileUnderneath().islandId == desyncIslandId ||
        Player.GetInstance().GetSTileUnderneath().islandId == desyncAnchoredIslandId));
    }
    #endregion
}
