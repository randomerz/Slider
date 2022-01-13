using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGrid : MonoBehaviour
{
    public static SGrid current { get; private set; }

    public class OnGridMoveArgs : System.EventArgs
    {
        public STile[,] grid;
    }
    public static event System.EventHandler<OnGridMoveArgs> OnGridMove; // IMPORTANT: this is in the background -- you might be looking for SGridAnimator.OnTileMove

    private STile[,] grid;
    private STile[,] altGrid;
    private SGridBackground[,] bgGrid;


    protected Area myArea;
    // Set in inspector 
    public int width;
    public int height;
    [SerializeField] private STile[] stiles;
    [SerializeField] private STile[] altStiles;
    [SerializeField] private SGridBackground[] bgGridTiles;
    [SerializeField] private SGridAnimator gridAnimator;
    [SerializeField] protected string targetGrid; // format: 123456789 for  1 2 3
                                                  //                        4 5 6
                                                  //              (0, 0) -> 7 8 9

    protected void Awake()
    {
        current = this;

        LoadGrid();
        SetBGGrid(bgGridTiles);
    }


    public STile[,] GetGrid()
    {
        return grid;
    }

    public STile[,] GetAltGrid()
    {
        return altGrid;
    }

    public SGridBackground[,] GetBGGrid()
    {
        return bgGrid;
    }

    public void SetGrid(int[,] puzzle)
    {
        if (puzzle.Length != grid.Length)
        {
            Debug.LogWarning("Tried to SetGrid(int[,]), but provided puzzle was of a different length!");
        }

        STile[,] newGrid = new STile[width, height];
        STile next = null;

        int playerIsland = Player.GetStileUnderneath();
        Vector3 playerOffset = Player.GetPosition() - GetStile(playerIsland).transform.position;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Debug.Log(puzzle[x, y]);
                if (puzzle[x, y] == 0)
                    next = GetStile(width * height);
                else
                    next = GetStile(puzzle[x, y]);

                next.x = x;
                next.y = y;
                newGrid[x, y] = next;
                next.Init();
                UIArtifact.SetButtonPos(next.islandId, x, y);
            }
        }

        Player.SetPosition(GetStile(playerIsland).transform.position + playerOffset);

        grid = newGrid;

        OnGridMove += CheckCompletions;
        ArtifactTileButton.canComplete = true;
    }

    private void SetGrid(STile[] stiles, STile[] altStiles=null)
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

    private void SetBGGrid(SGridBackground[] bgGridTiles) 
    {
        bgGrid = new SGridBackground[width, height];
        for (int i = 0; i < bgGridTiles.Length; i++)
        {
            bgGrid[i % width, i / width] = bgGridTiles[i];
        }
    }

    // public virtual void SetSGrid(SGrid other) { 
    //     // grid = other.grid;
    //     // altGrid = other.altGrid;
    //     // bgGrid = other.bgGrid;

    //     // inGame grid[stored.x, stored.y] = inGame stile(stored.islandId)
    //     STile[,] newGrid = new STile[width, height];
    //     foreach (STile t in other.stiles)
    //     {
    //         STile currentSTile = GetStile(t.islandId); 
    //         newGrid[t.x, t.y] = currentSTile;
    //         Debug.Log(t.x + ", " + t.y + ": " + t.islandId);
    //         currentSTile.SetSTile(t);
    //     }
    //     grid = newGrid;

    //     if (altStiles != null && altStiles.Length == width * height)
    //     {
    //         STile[,] newAltGrid = new STile[width, height];
    //         foreach (STile t in altStiles)
    //         {
    //             STile currentSTile = GetAltStile(t.islandId); 
    //             newAltGrid[t.x, t.y] = currentSTile;
    //             currentSTile.SetSTile(t);
    //         }
    //         altGrid = newAltGrid;
    //     }

    //     if (bgGridTiles != null && bgGridTiles.Length == width * height)
    //     {
    //         bgGrid = new SGridBackground[width, height];
    //         for (int i = 0; i < bgGridTiles.Length; i++)
    //         {
    //             bgGrid[i % width, i / width] = bgGridTiles[i];
    //         }
    //     }


    //     targetGrid = other.targetGrid;
    // }

    // Returns a string like:   123_6##_3#2
    // for a grid like:  1 2 3
    //                   6 . .
    //        (0, 0) ->  3 . 2
    public static string GetGridString() // todo? GetAltGridString()
    {
        string s = "";
        for (int y = current.height - 1; y >= 0; y--)
        {
            for (int x = 0; x < current.width; x++)
            {
                if (current.grid[x, y].isTileActive)
                    s += current.grid[x, y].islandId;
                else
                    s += "#";
            }
            if (y != 0)
            {
                s += "_";
            }
        }
        return s;
    }

    public STile GetStile(int islandId)
    {
        foreach (STile t in stiles)
            if (t.islandId == islandId)
                return t;
                
        return null;
    }

    public STile GetAltStile(int islandId)
    {
        foreach (STile t in altStiles)
            if (t.islandId == islandId)
                return t;
                
        return null;
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
                return;
            }
        }
    }



    public virtual void SaveGrid() 
    { 
        Debug.Log("Saving data for " + myArea);
        Debug.Log("this is: " + this);
        GameManager.saveSystem.SaveSGridData(myArea, this);
        GameManager.saveSystem.SaveMissions(new Dictionary<string, bool>());
    }

    public virtual void LoadGrid() 
    { 
        Debug.Log("Loading grid...");

        SGridData sgridData = GameManager.saveSystem.GetSGridData(myArea);
        Dictionary<string, bool> loadedMissions = GameManager.saveSystem.GetMissions(new List<string>());

        if (sgridData == null) {
            SetGrid(stiles, altStiles);
            return;
        }

        Debug.Log("Loading saved data for " + myArea + "...");

        STile[,] newGrid = new STile[width, height];
        foreach (SGridData.STileData td in sgridData.grid) 
        {
            STile stile = GetStile(td.islandId); 
            newGrid[td.x, td.y] = stile;
            stile.SetSTile(td.isTileActive, td.x, td.y);
        }
        grid = newGrid;

        if (altGrid != null && altGrid.Length == width * height) {
            STile[,] newAltGrid = new STile[width, height];
            foreach (SGridData.STileData td in sgridData.altGrid)
            {
                STile stile = GetStile(td.islandId); 
                newAltGrid[td.x, td.y] = stile;
                stile.SetSTile(td.isTileActive, td.x, td.y);
            }
            altGrid = newAltGrid;
        }

        // temporary ?
        GameObject.Find("Player").transform.position = GameManager.saveSystem.GetPlayerPos(myArea);
    }

    protected static void CheckCompletions(object sender, SGrid.OnGridMoveArgs e)
    {
        // ineffecient lol
        for (int x = 0; x < current.width; x++) {
            for (int y = 0; y < current.width; y++) {
                // int tid = current.targetGrid[x, y];
                int tid = current.targetGrid[(current.height - y - 1) * current.width + x];
                UIArtifact.SetButtonComplete(tid, current.grid[x, y].islandId == tid);
            }
        }
        // UIArtifact.SetButtonComplete(1, current.grid[0, 0].islandId == 1);
        // UIArtifact.SetButtonComplete(5, current.grid[1, 0].islandId == 5);
        // UIArtifact.SetButtonComplete(3, current.grid[2, 0].islandId == 3);
        // UIArtifact.SetButtonComplete(8, current.grid[0, 1].islandId == 8);
        // UIArtifact.SetButtonComplete(9, current.grid[1, 1].islandId == 9);
        // UIArtifact.SetButtonComplete(7, current.grid[2, 1].islandId == 7);
        // UIArtifact.SetButtonComplete(6, current.grid[0, 2].islandId == 6);
        // UIArtifact.SetButtonComplete(2, current.grid[1, 2].islandId == 2);
        // UIArtifact.SetButtonComplete(4, current.grid[2, 2].islandId == 4);
    }
}
