using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGrid3D : SGrid
{
    //L: The grid the player is currently interacting with (only 1 active at a time)
    public static SGrid3D current { get; private set; }

    public class OnGridMoveArgs : System.EventArgs
    {
        public STile3D[,,] grid;
    }
    public static event System.EventHandler<OnGridMoveArgs> OnGridMove; // IMPORTANT: this is in the background -- you might be looking for SGridAnimator.OnSTileMove

    protected STile3D[,,] grid;
    protected SGridBackground3D[,,] bgGrid;

    // Set in inspector 
    public int width;
    public int length;
    public int height;
    [SerializeField] private STile3D[] stiles;
    [SerializeField] private SGridBackground3D[] bgGridTiles;
    [SerializeField] private SGridAnimator3D gridAnimator;
    //L: This is the end goal for the slider puzzle, set in the inspector.
    //It is derived from the order of tiles in the puzzle doc. (EX: 624897153 for the starting Village)
    [SerializeField] protected string targetGrid = "*********"; // format: 123456789 for  1 2 3
                                                  //                                      4 5 6
                                                  //              (0, 0) ->               7 8 9

    public Collectible[] collectibles;
    protected Area myArea; // don't forget to set me!

    protected void Awake()
    {

        current = this;
        LoadGrid();
        SetBGGrid(bgGridTiles);

        if (targetGrid.Length == 0)
            Debug.LogWarning("Grid's target (end goal) is empty!");
        
        if (myArea == Area.None)
            Debug.LogWarning("Area isn't set!");

        // OnGridMove += CheckCompletions;
    }


    public STile3D[,,] GetGrid()
    {
        return grid;
    }

    public SGridBackground3D[,,] GetBGGrid()
    {
        return bgGrid;
    }

    /*L: Sets the position of STiles in the grid according to their ids.
    Note: This updates all of the STiles according to the ids in the given array (unlike the other imp., which leaves the STiles in the same positions)
    * 
    * This is useful for reshuffling the grid.
    */
    public void SetGrid(int[,,] puzzle)
    {
        if (puzzle.Length != grid.Length)
        {
            Debug.LogWarning("Tried to SetGrid(int[,]), but provided puzzle was of a different length!");
        }

        STile3D[,,] newGrid = new STile3D[width, length, height];
        STile3D next = null;

        int playerIsland = Player.GetStileUnderneath();
        Vector3 playerOffset = Player.GetPosition() - GetStile(playerIsland).transform.position;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                for(int z = 0; x < height; z++)
                {
                    //Debug.Log(puzzle[x, y]);
                    if (puzzle[x, y, z] == 0)
                        next = GetStile(width * length);
                    else
                        next = GetStile(puzzle[x, y, z]);

                    next.x = x;
                    next.y = y;
                    next.z = z;
                    newGrid[x, y, z] = next;
                        next.Init();
                    UIArtifact3D.SetButtonPos(next.islandId, x, y, z);
                }
            }
        }

        Player.SetPosition(GetStile(playerIsland).transform.position + playerOffset);

        grid = newGrid;

        // OnGridMove += CheckCompletions; // Handled in the specific grids
        // ArtifactTileButton.canComplete = true;
    }

    /*
     * L: Populates the grid[,] array with the given stiles.
     * This is what initially loads in the grid at the start of the scene if a grid is not already saved.
     */
    private void SetGrid(STile3D[] stiles)
    {
        if (stiles.Length != width * length)
        {
            Debug.LogError("Only " + stiles.Length + " found when initializing grid! " +
                "Expected " + width * length + ".");
            return;
        }

        grid = new STile3D[width, length, height];
        foreach (STile3D t in stiles)
        {
            grid[t.x, t.y, t.z] = t;
        }
    }

    private void SetBGGrid(SGridBackground3D[] bgGridTiles) 
    {
        bgGrid = new SGridBackground3D[width, length, height];
        for (int i = 0; i < bgGridTiles.Length; i++)
        {
            bgGrid[i % width, i / width, i / (width * length)] = bgGridTiles[i];
        }
    }


    //IDK HOW IM GONNA IMPLEMENT THIS YET 

    // Returns a string like:   123_6##_4#5
    // for a grid like:  1 2 3
    //                   6 . .
    //        (0, 0) ->  4 . 5
    public static string GetGridString()
    {
        
        string s = "";/* 
        for (int y = current.length - 1; y >= 0; y--)
        {
            for (int x = 0; x < current.width; x++)
            {
                if (current.grid[x, y, z].isTileActive)
                    s += current.grid[x, y, z].islandId;
                else
                    s += "#";
            }
            if (y != 0)
            {
                s += "_";
            }
        }*/
        return s;
    }

    //L: islandId is the id of the corresponding tile in the puzzle doc
    public STile3D GetStile(int islandId)
    {
        foreach (STile3D t in stiles)
            if (t.islandId == islandId)
                return t;
        return null;
    }

    //C: returns a list of active stiles
    public List<STile3D> GetActiveTiles()
    {
        List<STile3D> stileList = new List<STile3D>();
        foreach(STile3D tile in stiles)
            if(tile.isTileActive)
                stileList.Add(tile);
        return stileList;
    }

    //L: This mainly checks if any of the tiles involved in SMove3D 
    //D: this is should also not really be relied on
    public bool CanMove(SMove3D move)
    {
        foreach (Movement m in move.moves)
        {
            if (!grid[m.startLoc.x, m.startLoc.y, m.startLoc.z].CanMove(m.endLoc))
            {
                return false;
            }
        }
        return true;
    }

    public Collectible GetCollectible(string name)
    {
        foreach (Collectible c in collectibles)
        {
            if (c.GetName() == name)
            {
                return c;
            }
        }

        return null;
    }

    public void ActivateCollectible(string name)
    {
        GetCollectible(name).gameObject.SetActive(true);
    }
    public void ActivateSliderCollectible(int sliderId)
    {
        if (!PlayerInventory.Contains("Slider " + sliderId, myArea)) 
        {
            GetCollectible("Slider " + sliderId).gameObject.SetActive(true);
            AudioManager.Play("Puzzle Complete");
        }
    }

    public Area GetArea() 
    {
        return myArea;
    }

    // Make sure to check if you CanMove() before moving
    //L: Updates internal state (the grid[,]) based on result of SMove. See Move in SGridAnimator for the actual moving of the tiles.
    public void Move(SMove3D move)
    {

        gridAnimator.Move(move);

        STile3D[,,] newGrid = new STile3D[width, length, height];
        System.Array.Copy(grid, newGrid, width * length * height);
        foreach (Movement m in move.moves)
        {
            //grid[m.x, m.y].SetGridPosition(m.z, m.w);
            newGrid[m.endLoc.x, m.endLoc.y, m.endLoc.z] = grid[m.startLoc.x, m.startLoc.y, m.startLoc.z];
            //Debug.Log("Setting " + m.x + " " + m.y + " to " + m.z + " " + m.w);
        }
        grid = newGrid;

        OnGridMove?.Invoke(this, new OnGridMoveArgs { grid = grid });
    }



    public void EnableStile(int islandId)
    {
        foreach (STile3D s in grid)
        {
            if (s.islandId == islandId)
            {
                EnableStile(s);
                return;
            }
        }
    }

    public virtual void EnableStile(STile3D stile)
    {
        stile.SetTileActive(true);
        UIArtifact.AddButton(stile.islandId);
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
            SetGrid(stiles);
            return;
        }

        Debug.Log("Loading saved data for " + myArea + "...");

        // setting grids... similar to initialization
        STile3D[,,] newGrid = new STile3D[width, length, height];
        foreach (SGridData.STileData td in sgridData.grid) 
        {
            STile3D stile = GetStile(td.islandId); 
            newGrid[td.x, td.y, td.z] = stile;
            stile.SetSTile(td.isTileActive, td.x, td.y, td.z);
        }
        grid = newGrid;
    }


    protected static void CheckCompletions(object sender, SGrid.OnGridMoveArgs e)
    {
        // Debug.Log("Checking completions!");
        // ineffecient lol
        for (int x = 0; x < current.width; x++) {
            for (int y = 0; y < current.length; y++) {
                for (int z = 0; z < current.height; z++) {
                    string tids = current.targetGrid[(current.length - y - 1) * current.width + x].ToString();
                    if (tids == "*") 
                    {
                        UIArtifact3D.SetButtonComplete(current.grid[x, y, z].islandId, true);
                    }
                    else {
                        int tid = int.Parse(tids);
                        UIArtifact3D.SetButtonComplete(tid, current.grid[x, y, z].islandId == tid);
                    }
                }
            }
        }
    }
}
