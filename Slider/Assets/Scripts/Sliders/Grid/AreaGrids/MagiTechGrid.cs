using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiTechGrid : SGrid
{
    
    public static MagiTechGrid Instance => SGrid.Current as MagiTechGrid;

    public int gridOffset = 100; //C: The X distance between the present and past grid

    [SerializeField] private NPC lightningBoi;
    [SerializeField] private NPC fireBoi;
    [SerializeField] private NPC hungryBoi;
    [SerializeField] private DesyncItem desyncBurger;

    private Dictionary<Area, bool> gems;
    private bool fireStool;
    private bool lightningStool;

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

        AudioManager.PlayMusic("MagiTech");
        UIEffects.FadeFromBlack();
        gems = new Dictionary<Area, bool>();
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

    public override void Save()
    {
        base.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
    }

    public static bool IsInPast(Transform transform)
    {
        return transform.position.x > 67;
    }

    #endregion

    private List<Collider2D> GetCollidingItems(GameObject obj)
    {
        ContactFilter2D filter = new()
        {
            layerMask = LayerMask.GetMask("Item"),
            useLayerMask = true
        };

        BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
        List<Collider2D> list = new();
        collider.OverlapCollider(filter, list);
        return list;
    }

    public void HasTwoBurgers(Condition c)
    {
        if (!desyncBurger.IsDesynced)
        {
            c.SetSpec(false);
            return;
        }

        bool hasBurger = false;
        bool hasDesyncBurger = false;

        foreach (Collider2D hit in GetCollidingItems(hungryBoi.gameObject))
        {
            if (hit != null)
            {
                Item item = hit.GetComponent<Item>();
                //Debug.Log(item.itemName);
                if (item.itemName == "Burger") hasBurger = true;
                else if (item.itemName == desyncBurger.itemName) hasDesyncBurger = true;
            }
        }
        c.SetSpec(hasBurger && hasDesyncBurger);
    }

    public void FireHasStool(Condition c)
    {
        if (gems.GetValueOrDefault(Area.Mountain))
        {
            c.SetSpec(true);
            return;
        }
        foreach (Collider2D hit in GetCollidingItems(fireBoi.gameObject))
        {
            if (hit != null)
            {
                Item item = hit.GetComponent<Item>();
                if (item.itemName == "Step Stool")
                {
                    c.SetSpec(true);
                    return;
                }
            }
        }
        c.SetSpec(false);
    }
    public void LightningHasStool(Condition c)
    {
        if (gems.GetValueOrDefault(Area.Factory))
        {
            c.SetSpec(true);
            return;
        }

        foreach (Collider2D hit in GetCollidingItems(lightningBoi.gameObject))
        {
            if (hit != null)
            {
                Item item = hit.GetComponent<Item>();
                if (item.itemName == "Step Stool")
                {
                    c.SetSpec(true);
                    return;
                }
            }
        }
        c.SetSpec(false);
    }

    #region Gem Conds
    public void HasOceanGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Ocean, false));
    public void HasMilitaryGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Military, false));
    public void HasFactoryGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Factory, false));
    public void HasMountainGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Mountain, false));
    public void HasVillageGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Village, false));
    public void HasCavesGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Caves, false));
    public void HasDesertGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Desert, false));
    public void HasJungleGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Jungle, false));
    public void HasMagiTechGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.MagiTech, false));

    #endregion
    public void HasAllGems(Condition c)
    {
        foreach (bool b in gems.Values)
        {
            if (!b)
            {
                c.SetSpec(false);
                return;
            }
        }
        c.SetSpec(true);
    }

    public void TurnInGem(Item item)
    {
        if (Enum.TryParse(item.itemName, out Area itemNameAsEnum))
        {
            gems.Add(itemNameAsEnum, true);
            //Funni turn-in coroutine
            PlayerInventory.RemoveAndDestroyItem();
            //item.gameObject.SetActive(false);
            Debug.Log(itemNameAsEnum);
        }
    }

    public void TurnInMountory()
    {
        gems.Add(Area.Factory, true);
        gems.Add(Area.Mountain, true);
        PlayerInventory.RemoveAndDestroyItem();
    }
}
