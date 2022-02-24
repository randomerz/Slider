using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiTechGrid : SGrid
{
    public static MagiTechGrid instance;

    [SerializeField] private STile[] altStiles;

    [SerializeField] private STile[,] altGrid;

    private new void Awake()
    {
        myArea = Area.MagiTech;

        foreach (Collectible c in collectibles)
        {
            c.SetArea(myArea);
        }

        base.Awake();

        instance = this;
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

        AudioManager.PlayMusic("Connection");
        UIEffects.FadeFromBlack();
    }

    public override void SaveGrid()
    {
        base.SaveGrid();
    }

    public override void LoadGrid()
    {
        base.LoadGrid();

        // Should look into linking this into the save/load system later
        SetAltGrid(altStiles);
    }

    /*
     * L: Populates the altGrid[,] array with the given stiles.
     */
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

        /*foreach (STile tile in altGrid)
        {
            tile.SetTileActive(false);
        }
        foreach (STile tile in grid)
        {
            tile.SetTileActive(true);
        }*/

    }
}
