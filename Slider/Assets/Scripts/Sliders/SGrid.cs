using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGrid : MonoBehaviour
{
    public static SGrid current {   // "SGrid.current" will return the current active SGrid
        get         { return currentSGrid; }
        private set { currentSGrid = value; }
    }
    private static SGrid currentSGrid;

    private STile[,] grid;
    private STile[,] altGrid;

    // Set in inspector 
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private STile[] stiles;
    [SerializeField] private STile[] altStiles;

    private void Awake()
    {
        currentSGrid = this;

        SetGrid(stiles, altStiles);
    }

    void Start()
    {
        
    }


    public static STile[,] GetGrid()
    {
        return currentSGrid.grid;
    }

    public static STile GetStile(int islandId)
    {
        foreach (STile t in currentSGrid.stiles)
        {
            if (t.islandId == islandId)
            {
                return t;
            }
        }
        return null;
    }

    private void SetGrid(STile[] stiles, STile[] altStiles)
    {
        if (stiles.Length != width * height)
        {
            Debug.LogError("Only " + stiles.Length + " found when initializing grid! " +
                "Expected " + width * height + ".");
            return;
        }

        grid = new STile[width, height];
        foreach (STile t in stiles)
        {
            grid[t.x, t.y] = t;
        }

        if (altStiles != null && altStiles.Length == width * height)
        {
            altGrid = new STile[width, height];
            foreach (STile t in altStiles)
            {
                altGrid[t.x, t.y] = t;
            }
        }
    }

    public bool CanMove(SMove move)
    {
        foreach (Vector4Int m in move.moves)
        {
            if (!grid[m.x, m.y].CanMove(m.z, m.w))
            {
                return false;
            }
        }

        return true;
    }

    // Make sure to check if you CanMove() before moving
    public void Move(SMove move)
    {
        foreach (Vector4Int m in move.moves)
        {
            grid[m.x, m.y].SetPosition(m.z, m.w);
        }
    }
}
