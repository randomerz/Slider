using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiTechGrid : SGrid
{
    public static MagiTechGrid instance;

    public int GridOffset = 100; //C: The X distance between the present and past grid

    /* C: The Magitech grid is a 6 by 3 grid. The left 9 STiles represent the present,
    and the right 9 STiles represent the past. The past tile will have an islandID
    exactly 9 more than its corresponding present tile. Note that in strings, the past tiles
    will be reprsented with the characters A-I so they can retain a length of 1. *THIS HAS NOT BEEN PROPERLY IMPLEMENTED YET

    A Magitech grid might look like this

        1 2 3   A B C
        4 5 6   D E F
        7 8 9   G H I

    */

    [SerializeField] private STile[] altStiles;

    [SerializeField] private STile[,] altGrid;

    public override void Init()
    {
        myArea = Area.MagiTech;

        foreach (Collectible c in collectibles)
        {
            c.SetArea(myArea);
        }

        base.Init();

        instance = this;
        SetSingleton();
    }

    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("MagiTech");
        UIEffects.FadeFromBlack();
    }

    public override void Save()
    {
        base.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);

        // Should look into linking this into the save/load system later
      //  SetAltGrid(altStiles);

        // We want the colliders in the altGrid disabled so they don't push the player around
    //   StartCoroutine(ISetAltGridCollidersToTrigger());
        /*foreach (STile tile in altGrid)
        {
            tile.SetTileActive(false);

            tile.sliderCollider.isTrigger = true;
        }*/
    }
/*
    private IEnumerator ISetAltGridCollidersToTrigger()
    {
        yield return new WaitForEndOfFrame();
        foreach (STile tile in altGrid)
        {
            tile.SetTileActive(false);

            tile.sliderCollider.isTrigger = true;
        }
    }

    /*
     * L: Populates the altGrid[,] array with the given stiles.
     *
    private void SetAltGrid(STile[] stiles)
    {
        if (stiles.Length != width * height)
        {
            Debug.LogError("Only " + stiles.Length + " found when initializing altGrid! " +
                "Expected " + width * height + ".");
            return;
        }

        altGrid = new STile[width, height];
        foreach (STile t in stiles)
        {
            altGrid[t.x, t.y] = t;
        }
    }

    public void SwapGrids()
    {
        STile[,] temp = grid;
        grid = altGrid;
        altGrid = temp;

        foreach (STile tile in altGrid)
        {
            tile.SetTileActive(false);
            tile.sliderCollider.isTrigger = true;
        }
        foreach (STile tile in grid)
        {
            // We only enable tiles in the current grid if they have been collected already
            tile.SetTileActive(tile.isTileCollected);
        }
    }
*/
    // See STile.isTileCollected for an explanation
    public override void CollectSTile(int islandId)
    {
        foreach (STile s in grid)
        {
            //Debug.Log(s.islandId);
            if (s.islandId == islandId || s.islandId - 9 == islandId)
            {
                CollectStile(s);
                //break;
            }
        }
        /*foreach (STile s in altGrid)
        {
            if (s.islandId == islandId)
            {
                CollectStile(s);
                break;
            }
        }*/
    }/*

    public bool AltGridCanMove(SMove move)
    {
        foreach (Movement m in move.moves)
        {
            if (!altGrid[m.startLoc.x, m.startLoc.y].CanMove(m.startLoc.x, m.startLoc.y))
            {
                return false;
            }
        }
        return true;
    }
/*
    public override void Move(SMove move)
    {
        base.Move(move);

        // Move the altGrid to match
        if (AltGridCanMove(move))
        {
            gridAnimator.Move(move, altGrid);

            STile[,] newAltGrid = new STile[width, height];
            System.Array.Copy(altGrid, newAltGrid, width * height);
            foreach (Movement m in move.moves)
            {
                //grid[m.x, m.y].SetGridPosition(m.z, m.w);
                newAltGrid[m.endLoc.x, m.endLoc.y] = altGrid[m.startLoc.x, m.startLoc.y];
                //Debug.Log("Setting " + m.x + " " + m.y + " to " + m.z + " " + m.w);
            }
            altGrid = newAltGrid;
        }
    }
    */
}
