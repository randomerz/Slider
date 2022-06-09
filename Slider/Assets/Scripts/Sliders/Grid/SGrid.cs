using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGrid : MonoBehaviour, ISavable
{
    //L: The grid the player is currently interacting with (only 1 active at a time)
    public static SGrid current { get; private set; }
    private bool didInit;

    public class OnGridMoveArgs : System.EventArgs
    {
        public STile[,] grid;
    }
    public static event System.EventHandler<OnGridMoveArgs> OnGridMove; // IMPORTANT: this is in the background -- you might be looking for SGridAnimator.OnSTileMove

    public class OnSTileEnabledArgs : System.EventArgs
    {
        public STile stile;
    }
    public static event System.EventHandler<OnSTileEnabledArgs> OnSTileEnabled;

    public class OnSTileCollectedArgs : System.EventArgs
    {
        public STile stile;
    }
    public static event System.EventHandler<OnSTileCollectedArgs> OnSTileCollected;

    protected STile[,] grid;
    protected SGridBackground[,] bgGrid;
    public int[,] realigningGrid;

    // Set in inspector 
    public int width;
    public int height;
    [SerializeField] protected STile[] stiles;
    [SerializeField] private SGridBackground[] bgGridTiles;
    [SerializeField] protected SGridAnimator gridAnimator;
    //L: This is the end goal for the slider puzzle, set in the inspector.
    //It is derived from the order of tiles in the puzzle doc. (EX: 624897153 for the starting Village)
    [SerializeField] protected string targetGrid = "*********"; // format: 123456789 for  1 2 3
                                                  //                                      4 5 6
                                                  //              (0, 0) ->               7 8 9

    public string TargetGrid
    {
        get { return targetGrid; }
    }

    public Collectible[] collectibles;
    [SerializeField] protected Area myArea; // don't forget to set me!
    public Area MyArea { get => myArea; }


    protected void Awake()
    {
        current = this;

        if (!didInit)
            Init();
    }

    protected virtual void Start() 
    {
        foreach (Collectible c in collectibles)
        {
            if (PlayerInventory.Contains(c))
            {
                c.gameObject.SetActive(false);
            }

        }

        UIArtifactWorldMap.SetAreaStatus(myArea, ArtifactWorldMapArea.AreaStatus.oneBit);
    }

    public void SetSingleton()
    {
        current = this;
    }

    public virtual void Init()
    {
        didInit = true;

        SaveSystem.Current.SetLastArea(myArea);

        Load(SaveSystem.Current); // DC: this won't cause run order issues right :)
        SetBGGrid(bgGridTiles);

        if (targetGrid.Length == 0)
            Debug.LogWarning("Grid's target (end goal) is empty!");

        if (myArea == Area.None)
            Debug.LogWarning("Area isn't set!");

        // OnGridMove += CheckCompletions;
    }

    public STile[,] GetGrid()
    {
        return grid;
    }

    public SGridBackground[,] GetBGGrid()
    {
        return bgGrid;
    }

    /*L: Sets the position of STiles in the grid according to their ids.
    Note: This updates all of the STiles according to the ids in the given array (unlike the other imp., which leaves the STiles in the same positions)
    * 
    * This is useful for reshuffling the grid.
    * 
    */
    public void SetGrid(int[,] puzzle)
    {
        if (puzzle.Length != grid.Length)
        {
            Debug.LogWarning("Tried to SetGrid(int[,]), but provided puzzle was of a different length!");
        }

        STile[,] newGrid = new STile[width, height];
        STile next = null;

        // We might not need this getunderstile stuff anymore now that we actually child player to STiles!
        STile playerSTile = Player.GetStileUnderneath();
        Vector3 playerOffset = playerSTile ? Player.GetPosition() - playerSTile.transform.position : Vector3.zero;

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

        if (playerSTile != null)
            Player.SetPosition(playerSTile.transform.position + playerOffset);

        grid = newGrid;
    }

    /*
     * L: Populates the grid[,] array with the given stiles.
     * This is what initially loads in the grid at the start of the scene if a grid is not already saved.
     */
    private void SetGrid(STile[] stiles)
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
    }

    private void SetBGGrid(SGridBackground[] bgGridTiles) 
    {
        bgGrid = new SGridBackground[width, height];
        for (int i = 0; i < bgGridTiles.Length; i++)
        {
            bgGrid[i % width, i / width] = bgGridTiles[i];
        }
    }

    /* C: converts integers >=10 to characters
     * 10 = A, 11 = B, etc.
     * Integers 0-9 are left untouched (note that they
     * are now characters)
     */
    public static char IntToChar(int num)
    {
        return (num > 9) ? (char)('A' -  10 + num) : (char)('0' + num);
    }

    public static int CharToInt(char c)
    {
        return (c > '9') ? (c - 'A' +  10) : (c - '0');
    }

    // Returns a string like:   123_6##_4#5
    // for a grid like:  1 2 3
    //                   6 . .
    //        (0, 0) ->  4 . 5
    public static string GetGridString()
    {
        string s = "";
        for (int y = current.height - 1; y >= 0; y--)
        {
            for (int x = 0; x < current.width; x++)
            {
                if (current.grid[x, y].isTileActive)
                    s += IntToChar(current.grid[x, y].islandId);
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

    public static int[,] GridStringToSetGridFormat(string gridstring)
    {
        //Chen: This in theory should work for other grids? This is mostly used with Scroll of Realigning stuff.
        int[,] gridFormat = new int[current.width, current.height];
        for (int x = current.width - 1; x >= 0; x--)
        {
            for (int y = 0; y < current.height; y++)
            {
                gridFormat[y, (current.width - 1 - x)] = CharToInt(gridstring[(x * current.height) + y]);
            }
        }
        return gridFormat;
    }

    //L: islandId is the id of the corresponding tile in the puzzle doc
    public STile GetStile(int islandId)
    {
        foreach (STile t in stiles)
            if (t.islandId == islandId)
                return t;
                
        return null;
    }

    //C: returns a list of active stiles
    public List<STile> GetActiveTiles()
    {
        List<STile> stileList = new List<STile>();
        foreach(STile tile in stiles)
            if(tile.isTileActive)
                stileList.Add(tile);
        return stileList;
    }

    /// <summary>
    /// Returns the number of STiles collected in the current SGrid.
    /// </summary>
    /// <returns></returns>
    public virtual int GetNumTilesCollected() {
        int numCollected = 0;
        foreach (STile tile in stiles)
        {
            if (tile.isTileCollected)
            {
                numCollected++;
            }
        }
        return numCollected;
    }
    /// <summary>
    /// Returns the number of STiles available in the current SGrid.
    /// </summary>
    /// <returns></returns>
    public virtual int GetTotalNumTiles()
    {
        return width * height;
    }

    //S: copy of Player's GetStileUnderneath for the tracker
    // DC: This will prefer an GameObjs parented STile if it has one
    public STile GetStileUnderneath(GameObject target)
    {
        float offset = grid[0, 0].STILE_WIDTH / 2f;
        float housingOffset = -150;
        
        STile stileUnderneath = null;
        STile currentStileUnderneath = target.GetComponentInParent<STile>(); // in case this obj is parented to something
        foreach (STile s in grid)
        {
            if (s.isTileActive && IsObjectInSTileBounds(target.transform.position, s.transform.position, offset, housingOffset))
            {
                if (currentStileUnderneath != null && s.islandId == currentStileUnderneath.islandId)
                {
                    // we are still on top of the same one
                    return currentStileUnderneath;
                }
                
                if (stileUnderneath == null || s.islandId < stileUnderneath.islandId)
                {
                    // in case where multiple overlap and none are picked, take the lowest number?
                    stileUnderneath = s;
                }
            }
        }
        return stileUnderneath;
    }

    private bool IsObjectInSTileBounds(Vector3 targetPos, Vector3 stilePos, float offset, float housingOffset)
    {
        if (stilePos.x - offset < targetPos.x && targetPos.x < stilePos.x + offset &&
           (stilePos.y - offset < targetPos.y && targetPos.y < stilePos.y + offset || 
            stilePos.y - offset + housingOffset < targetPos.y && targetPos.y < stilePos.y + offset + housingOffset))
        {
            return true;
        }
        return false;
    }


    //L: This mainly checks if any of the tiles involved in SMove 
    //D: this is should also not really be relied on
    public bool CanMove(SMove move)
    {
        foreach (Movement m in move.moves)
        {
            if (!grid[m.startLoc.x, m.startLoc.y].CanMove(m.startLoc.x, m.startLoc.y))
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
        if (!PlayerInventory.Contains(name, myArea))
        {
            GetCollectible(name)?.gameObject.SetActive(true);
        }
            
    }

    public void ActivateSliderCollectible(int sliderId)
    {
        if (!PlayerInventory.Contains("Slider " + sliderId, myArea)) 
        {
            //Debug.Log("Activated Collectible?");
            //Debug.Log(GetCollectible("Slider " + sliderId).gameObject.name);
            GetCollectible("Slider " + sliderId)?.gameObject.SetActive(true);
            AudioManager.Play("Puzzle Complete");
        }
    }

    public void GivePlayerTheCollectible(string name)
    {
        Debug.Log("Activating collectible " + name);
        if (GetCollectible(name) != null)
        {
            ActivateCollectible(name);
            GetCollectible(name).transform.position = Player.GetPosition();
            UIManager.CloseUI();
        }
    }

    public Area GetArea() 
    {
        return myArea;
    }

    // Make sure to check if you CanMove() before moving
    //L: Updates internal state (the grid[,]) based on result of SMove. See Move in SGridAnimator for the actual moving of the tiles.
    public virtual void Move(SMove move)
    {

        gridAnimator.Move(move);

        STile[,] newGrid = new STile[width, height];
        System.Array.Copy(grid, newGrid, width * height);
        foreach (Movement m in move.moves)
        {
            //grid[m.x, m.y].SetGridPosition(m.z, m.w);
            newGrid[m.endLoc.x, m.endLoc.y] = grid[m.startLoc.x, m.startLoc.y];
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
                EnableStile(s);
                return;
            }
        }
    }
    // See STile.isTileCollected for an explanation
    public virtual void CollectSTile(int islandId)
    {
        foreach (STile s in grid)
        {
            if (s.islandId == islandId)
            {
                CollectStile(s);
                return;
            }
        }
    }

    public virtual void EnableStile(STile stile, bool flickerButton=true)
    {
        stile.SetTileActive(true);
        UIArtifact.AddButton(stile, flickerButton);
        OnSTileEnabled?.Invoke(this, new OnSTileEnabledArgs { stile = stile });
    }
    // See STile.isTileCollected for an explanation
    public virtual void CollectStile(STile stile)
    {
        stile.isTileCollected = true;
        EnableStile(stile, true);
        OnSTileCollected?.Invoke(this, new OnSTileCollectedArgs { stile = stile });
    }

    public virtual bool TilesMoving()
    {
        List<STile> stiles = SGrid.current.GetActiveTiles();
        bool tilesAreMoving = false;
        foreach (STile stile in stiles)
        {
            if (stile.GetMovingDirection() != Vector2.zero)
            {
                tilesAreMoving = true;
            }
        }

        return tilesAreMoving;
    }

    public virtual void Save() 
    { 
        Debug.Log("Saving data for " + myArea);
        SaveSystem.Current.SaveSGridData(myArea, this);
    }

    //L: Used in the save system to load a grid as opposed to using SetGrid(STile[], STile[]) with default tiles positions.
    public virtual void Load(SaveProfile profile) 
    { 
        //Debug.Log("Loading grid...");

        SGridData sgridData = profile.GetSGridData(myArea);

        if (sgridData == null) {
            SetGrid(stiles);
            return;
        }

        Debug.Log("Loading saved data for " + myArea + "...");

        // setting grids... similar to initialization
        STile[,] newGrid = new STile[width, height];
        int[,] idGrid = new int[width, height];
        foreach (SGridData.STileData td in sgridData.grid) 
        {
            STile stile = GetStile(td.islandId); 
            newGrid[td.x, td.y] = stile;
            idGrid[td.x, td.y] = td.islandId;
            stile.SetSTile(td.isTileActive, td.x, td.y);

            UIArtifact.SetButtonPos(td.islandId, td.x, td.y);
        }
        grid = newGrid;
        realigningGrid = sgridData.realigningGrid;
    }

    public virtual void SaveRealigningGrid()
    {
        //Debug.Log("Saved!");
        realigningGrid = new int[current.width, current.height];
        //GedGridString but turns it to SetGrid format
        for (int x = 0; x < current.width; x++)
        {
            for (int y = 0; y < current.height; y++)
            {
                //Debug.Log(current.grid[x, y].islandId);
                realigningGrid[x,y] = grid[x, y].islandId;
            }
        }
        //Debug.Log(realigningGrid);
    }

    public virtual void LoadRealigningGrid()
    {
        //Debug.Log("Loaded!");
        if (realigningGrid == null)
        {
            //THIS SHOULD NOT BE HAPPENING
            Debug.LogError("realigningGrid is null!");
            return;
        }
        SetGrid(realigningGrid);
        realigningGrid = null;
    }

    public virtual void RearrangeGrid()
    {
        //Convert the target grid into the proper int[] and pass into setgrid
        current.SetGrid(GridStringToSetGridFormat(targetGrid));

        for (int x = 0; x < current.width; x++)
        {
            for (int y = 0; y < current.height; y++)
            {
                ArtifactTileButton artifactButton = UIArtifact.GetButton(x, y);
                UIArtifact.SetButtonComplete(artifactButton.islandId, true);
            }
        }
    }
    public bool HasRealigningGrid()
    {
        return realigningGrid != null;
    }
    protected static void UpdateButtonCompletions(object sender, System.EventArgs e)
    {
        current.UpdateButtonCompletionsHelper();
    }

    protected virtual void UpdateButtonCompletionsHelper()
    {
        for (int x = 0; x < current.width; x++) {
            for (int y = 0; y < current.height; y++) {
                // int tid = current.targetGrid[x, y];
                string tids = GetTileIdAt(x, y);
                ArtifactTileButton artifactButton = UIArtifact.GetButton(x, y);
                if (tids == "*") 
                {
                    // UIArtifact.SetButtonComplete(current.grid[x, y].islandId, true);
                    UIArtifact.SetButtonComplete(artifactButton.islandId, true);
                }
                else if (artifactButton != null) {
                    int tid = int.Parse(tids);
                    // UIArtifact.SetButtonComplete(tid, current.grid[x, y].islandId == tid);
                    UIArtifact.SetButtonComplete(artifactButton.islandId, artifactButton.islandId == tid);
                }
            }
        }
    }

    public static int GetNumButtonCompletions()
    {
        return current.GetNumButtonCompletionsHelper();
    }

    protected virtual int GetNumButtonCompletionsHelper()
    {
        int numComplete = 0;
        for (int x = 0; x < current.width; x++) {
            for (int y = 0; y < current.height; y++) {
                string tids = GetTileIdAt(x, y);
                ArtifactTileButton artifactButton = UIArtifact.GetButton(x, y);
                Debug.Log(x + " " + y);
                if (tids == "*") 
                {
                    numComplete += 1;
                }
                else {
                    int tid = int.Parse(tids);
                    //Debug.Log("tid: " + tid + " artifactButton: " + artifactButton);
                    if (artifactButton.islandId == tid)
                        numComplete += 1;
                }
            }
        }

        return numComplete;
    }

    protected IEnumerator CheckCompletionsAfterDelay(float t)
    {
        yield return new WaitForSeconds(t);

        UpdateButtonCompletions(this, null); // sets the final one to be complete
    }

    protected static string GetTileIdAt(int x, int y)
    {
        return current.targetGrid[(current.height - y - 1) * current.width + x].ToString();
    }
}
