using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGrid : MonoBehaviour
{
    //L: The grid the player is currently interacting with (only 1 active at a time)
    public static SGrid current { get; private set; }

    public class OnGridMoveArgs : System.EventArgs
    {
        public STile[,] grid;
    }
    public static event System.EventHandler<OnGridMoveArgs> OnGridMove; // IMPORTANT: this is in the background -- you might be looking for SGridAnimator.OnSTileMove

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
    //L: This is the end goal for the slider puzzle, set in the inspector.
    //It is derived from the order of tiles in the puzzle doc. (EX: 624897153 for the starting Village)
    [SerializeField] protected string targetGrid = "*********"; // format: 123456789 for  1 2 3
                                                  //                                      4 5 6
                                                  //              (0, 0) ->               7 8 9

    protected void Awake()
    {

        current = this;
        LoadGrid();
        SetBGGrid(bgGridTiles);

        if (targetGrid.Length == 0)
            Debug.LogWarning("Grid's target (end goal) is empty!");

        OnGridMove += CheckCompletions;
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

    /*L: Sets the position of STiles in the grid according to their ids.
    Note: This updates all of the STiles according to the ids in the given array (unlike the other imp., which leaves the STiles in the same positions)
    * 
    * This is useful for reshuffling the grid.
    */
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

    /*
     * L: Populates the grid[,] array with the given stiles.
     * This is what initially loads in the grid at the start of the scene if a grid is not already saved.
     */
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

    //L: islandId is the id of the corresponding tile in the puzzle doc
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

    //L: This mainly checks if any of the tiles involved in SMove 
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

    public Area GetArea() 
    {
        return myArea;
    }

    // Make sure to check if you CanMove() before moving
    //L: Updates internal state (the grid[,]) based on result of SMove. See Move in SGridAnimator for the actual moving of the tiles.
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
        GameManager.GetSaveSystem().SaveSGridData(myArea, this);
        GameManager.GetSaveSystem().SaveMissions(new Dictionary<string, bool>());
    }

    //L: Used in the save system to load a grid as opposed to using SetGrid(STile[], STile[]) with default tiles positions.
    public virtual void LoadGrid() 
    { 
         //Debug.Log("Loading grid...");

        SGridData sgridData = GameManager.GetSaveSystem().GetSGridData(myArea);
        Dictionary<string, bool> loadedMissions = GameManager.GetSaveSystem().GetMissions(new List<string>());

        if (sgridData == null) {
            SetGrid(stiles, altStiles);
            return;
        }

        Debug.Log("Loading saved data for " + myArea + "...");

        // setting grids... similar to initialization
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
        // GameObject.Find("Player").transform.position = GameManager.saveSystem.GetPlayerPos(myArea);
    }


    protected static void CheckCompletions(object sender, SGrid.OnGridMoveArgs e)
    {
        // ineffecient lol
        for (int x = 0; x < current.width; x++) {
            for (int y = 0; y < current.width; y++) {
                // int tid = current.targetGrid[x, y];
                string tids = current.targetGrid[(current.height - y - 1) * current.width + x].ToString();
                if (tids == "*") 
                {
                    UIArtifact.SetButtonComplete(current.grid[x, y].islandId, true);
                }
                else {
                    int tid = int.Parse(tids);
                    UIArtifact.SetButtonComplete(tid, current.grid[x, y].islandId == tid);
                }
            }
        }
    }
}
