using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiTechGrid : SGrid
{
    
    public static MagiTechGrid Instance => SGrid.Current as MagiTechGrid;

    public int gridOffset = 100; //C: The X distance between the present and past grid

    [SerializeField] private NPC hungryBoi;
    [SerializeField] private DesyncItem desyncBurger;

    private Dictionary<Area, bool> gems;

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

    public void HasTwoBurgers(Condition c)
    {
        if (desyncBurger.IsDesynced)
        {
            BoxCollider2D collider = hungryBoi.GetComponent<BoxCollider2D>();

            ContactFilter2D filter = new ContactFilter2D();
            filter.layerMask = LayerMask.GetMask("Item");
            filter.useLayerMask = true;

            List<Collider2D> list = new List<Collider2D>(); //Change this to however many items can be put inside hungry boi's collider
            collider.OverlapCollider(filter, list);

            bool hasBurger = false;
            bool hasDesyncBurger = false;

            foreach (Collider2D hit in list)
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
        else
        {
            c.SetSpec(false);
        }
    }

    #region Gem Conds
    public void hasOceanGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Ocean, false));
    public void hasMilitaryGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Military, false));
    public void hasFactoryGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Factory, false));
    public void hasMountainGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Mountain, false));
    public void hasVillageGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Village, false));
    public void hasCavesGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Caves, false));
    public void hasDesertGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Desert, false));
    public void hasJungleGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.Jungle, false));
    public void hasMagiTechGem(Condition c) => c.SetSpec(gems.GetValueOrDefault(Area.MagiTech, false));

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
        Area itemNameAsEnum;
        if (Enum.TryParse(item.itemName, out itemNameAsEnum))
        {
            gems.Add(itemNameAsEnum, true);
            //Funni turn-in coroutine
            PlayerInventory.RemoveItem();
            item.gameObject.SetActive(false);
        }

    }
}
