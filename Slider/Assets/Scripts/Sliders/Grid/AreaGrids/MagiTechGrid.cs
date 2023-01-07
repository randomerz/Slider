using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiTechGrid : SGrid
{
    
    public static MagiTechGrid Instance => SGrid.Current as MagiTechGrid;

    public int gridOffset = 100; //C: The X distance between the present and past grid

    [SerializeField] private Collider2D lightningBoi;
    [SerializeField] private Collider2D fireBoi;
    [SerializeField] private Collider2D hungryBoi;
    [SerializeField] private DesyncItem desyncBurger;

    private bool hasBurger;
    private bool hasDesyncBurger;
    private int numOres = 0;

    private ContactFilter2D contactFilter;

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
        UIEffects.FadeFromBlack();
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
        if (!desyncBurger.IsDesynced)
        {
            c.SetSpec(false);
            return;
        }

        foreach (Collider2D hit in GetCollidingItems(hungryBoi))
        {
            Item item = hit.GetComponent<Item>();
            if (item != null)
            {
                hasBurger = item.itemName == "Burger" || hasBurger;
                hasDesyncBurger = item.itemName == desyncBurger.itemName || hasDesyncBurger;
            }
        }
        c.SetSpec(hasBurger && hasDesyncBurger);
    }
    public void FireHasStool(Condition c)
    {
        if (SaveSystem.Current.GetBool("magiTechFactory"))
        {
            c.SetSpec(true);
            return;
        }
        foreach (Collider2D hit in GetCollidingItems(fireBoi))
        {
            Item item = hit.GetComponent<Item>();
            if (item != null && item.itemName == "Step Stool")
            {
                c.SetSpec(true);
                return;
            }
        }
        c.SetSpec(false);
    }
    public void LightningHasStool(Condition c)
    {
        if (SaveSystem.Current.GetBool("magiTechFactory"))
        {
            c.SetSpec(true);
            return;
        }

        foreach (Collider2D hit in GetCollidingItems(lightningBoi))
        {
            Item item = hit.GetComponent<Item>();
            if (item != null && item.itemName == "Step Stool")
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
    public void TurnInArtifact()
    {
        //Ensure Scroll of Realign is used
        if (!(UIArtifact.GetGridString() == "132_76#_548"))
        {
            return;
        }
        //Disable ability to open artifact
        UIArtifactMenus._instance.hasArtifact = false;
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
}
