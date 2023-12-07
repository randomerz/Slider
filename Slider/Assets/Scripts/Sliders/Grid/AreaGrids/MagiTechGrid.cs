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

    //True when the opposite tile is at a different location
    public bool DesyncActive = false;

    public class OnDesyncArgs : EventArgs
    {
        public int desyncIslandId;
        public Vector2Int desyncLocation;

        public OnDesyncArgs(int desyncIslandId, Vector2Int desyncLocation)
        {
            this.desyncIslandId = desyncIslandId;
            this.desyncLocation = desyncLocation;
        }
    }

    public static EventHandler<OnDesyncArgs> OnDesyncStartWorld;
    public static EventHandler<OnDesyncArgs> OnDesyncEndWorld;

    [SerializeField] private Collider2D fireStoolZoneCollider;
    [SerializeField] private Collider2D lightningStoolZoneCollider;
    private int numOres = 0;

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
        base.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        if(GetNumTilesCollected() >= 1)
            tabManager.EnableTab();
        if(profile.GetBool("magitechBridgeFixed"))
            LowerDrawbridge(true);
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
            }
            else if (desyncIslandId != -1)
            {
                DesyncActive = false;
                OnDesyncEndWorld?.Invoke(this, new(desyncIslandId, desyncLocation));
                desyncLocation = new Vector2Int(-1, -1);
                desyncIslandId = -1;
            }
        }
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
                OnDesyncStartWorld?.Invoke(this, new(desyncIslandId, desyncLocation));
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
                OnDesyncEndWorld?.Invoke(this, new(desyncIslandId, desyncLocation));
            }
        }
    }

    public static bool IsTileDesynced(STile tile)
    {
        if(tile == null) return false;
        var m = Current as MagiTechGrid;
        return m.DesyncActive && m.desyncIslandId == tile.islandId;
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

    public void HasOneOre(Condition c)
    {
        c.SetSpec(numOres == 1);
    }

    public void HasTwoOres(Condition c)
    {
        c.SetSpec(numOres == 2);
    }
    public void HasThreeOres(Condition c)
    {
        c.SetSpec(numOres == 3);
    }

    public void IncrementOres()
    {
        numOres++;
    }

    public void IsDesyncActive(Condition c)
    {
        c.SetSpec(DesyncActive);
    }

    public void IsPlayerOnDesyncTile(Condition c)
    {
        c.SetSpec(DesyncActive && 
        Player.GetInstance().GetSTileUnderneath() != null &&
        Player.GetInstance().GetSTileUnderneath().islandId == desyncIslandId);
    }
    #endregion
}
