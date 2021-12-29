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

    public class OnGridMoveArgs : System.EventArgs
    {
        public STile[,] grid;
    }
    public static event System.EventHandler<OnGridMoveArgs> OnGridMove; // IMPORTANT: this is in the background -- you might be looking for SGridAnimator.OnTileMove

    private STile[,] grid;
    private STile[,] altGrid;

    // Set in inspector 
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private STile[] stiles;
    [SerializeField] private STile[] altStiles;
    [SerializeField] private SGridAnimator gridAnimator;

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

    // Returns a string like: [123_6##_3#2]
    // for a grid like:  1 2 3
    //                   6 . .
    //        (0, 0) ->  3 . 2
    public static string GetGridString() // todo? GetAltGridString()
    {
        string s = "[";

        for (int y = current.height - 1; y >= 0; y--)
        {
            for (int x = 0; x < current.width; x++)
            {
                if (current.grid[x, y].isTileActive)
                    s += current.grid[x, y].islandId;
                else
                    s += "#";
            }
        }

        s += "]";

        return s;
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
        gridAnimator.Move(move);

        STile[,] newGrid = new STile[width, height];
        System.Array.Copy(grid, newGrid, width * height);
        foreach (Vector4Int m in move.moves)
        {
            //grid[m.x, m.y].SetGridPosition(m.z, m.w);
            newGrid[m.z, m.w] = grid[m.x, m.y];
            //Debug.Log("Setting " + m.x + " " + m.y + " to " + m.z + " " + m.w);
        }
        grid = newGrid;

        OnGridMove?.Invoke(this, new OnGridMoveArgs { grid = grid });
    }



    public void EnableStile(int islandId)
    {
        foreach (STile s in grid)
        {
            if (s.islandId == islandId)
            {
                s.SetTileActive(true);
                UIArtifact.AddButton(islandId);

                if (islandId == 9)
                {
                    Debug.Log("Found all 9 pieces!");
                    UIArtifact.SetButtonComplete(9, true);
                }
                return;
            }
        }
    }
    
}
