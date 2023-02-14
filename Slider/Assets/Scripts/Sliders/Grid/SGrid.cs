using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ** THIS CLASS HAS BEEN UPDATED TO USE THE NEW SINGLETON BASE CLASS. PLEASE REPORT NEW ISSUES YOU SUSPECT ARE RELATED TO THIS CHANGE TO TRAVIS AND/OR DANIEL! **
public class SGrid : Singleton<SGrid>, ISavable
{
    public class OnGridMoveArgs : System.EventArgs
    {
        public STile[,] oldGrid;
        public STile[,] grid;
    }

    public class OnSTileEnabledArgs : System.EventArgs
    {
        public STile stile;
    }

    public class OnSTileCollectedArgs : System.EventArgs
    {
        public STile stile;
    }

    public static event System.EventHandler<OnGridMoveArgs> OnGridMove; // IMPORTANT: this is in the background -- you might be looking for SGridAnimator.OnSTileMove
    public static event System.EventHandler<OnSTileEnabledArgs> OnSTileEnabled;
    public static event System.EventHandler<OnSTileCollectedArgs> OnSTileCollected;

    public static SGrid Current => _instance;
    public int Width => width;
    public int Height => height;
    public Area MyArea { get => myArea; }
    public string TargetGrid { get { return targetGrid; } }
    public bool CheckCompletion {
        get => checkCompletion;
        set {
            Debug.LogWarning("Check completion was set outside of SGrid!");
            checkCompletion = value;
        }
    }

    public int[,] realigningGrid;

    [SerializeField] protected int width;
    [SerializeField] protected int height;
    [SerializeField] protected int housingOffset = -150;
    [SerializeField] protected STile[] stiles;
    [SerializeField] protected SGridBackground[] bgGridTiles;
    [SerializeField] protected Collectible[] collectibles;
    [SerializeField] protected SGridAnimator gridAnimator;

    //L: This is the end goal for the slider puzzle.
    //It is derived from the order of tiles in the puzzle doc. (EX: 624897153 for the starting Village)
    // format: 123456789 for 1 2 3
    //                       4 5 6
    //                       7 8 9
    [SerializeField] protected string targetGrid = "*********";     

    [Tooltip("Don't forget to set me!")] [SerializeField] protected Area myArea;


    protected STile[,] grid;
    protected SGridBackground[,] bgGrid;
    protected bool checkCompletion;

    private bool didInit;

    protected void Awake()
    {
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
        
        if (checkCompletion)
        {
            UpdateButtonCompletions(this, null);
        }
        
        UIEffects.FadeFromBlack(() => {UIManager.SetCanOpenMenus(true);});
    }

    //For deriving classes: Make sure InitArea is called before Init!
    public virtual void Init()
    {
        didInit = true;
        InitializeSingleton(this);

        SaveSystem.Current.SetLastArea(myArea);

        Load(SaveSystem.Current); // DC: this won't cause run order issues right :)
        SetBGGrid(bgGridTiles);

        if (targetGrid.Length == 0)
            Debug.LogWarning("Grid's target (end goal) is empty!");

        if (myArea == Area.None)
            Debug.LogWarning("Area isn't set!");

        // OnGridMove += CheckCompletions;
    }

    public void InitArea(Area area)
    {
        myArea = area;
        foreach (Collectible c in collectibles)
        {
            c.SetArea(area);
        }
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

        STile[,] newGrid = new STile[Width, Height];
        STile next = null;

        // We might not need this getunderstile stuff anymore now that we actually child player to STiles!
        STile playerSTile = Player.GetInstance().GetSTileUnderneath();
        Vector3 playerOffset = playerSTile ? Player.GetPosition() - playerSTile.transform.position : Vector3.zero;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                //Debug.Log(puzzle[x, y]);
                if (puzzle[x, y] == 0)
                    next = GetStile(Width * Height);
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

        STile[,] old = grid;
        grid = newGrid;

        //OnGridMove.Invoke(this, new OnGridMoveArgs {
        //    oldGrid = old, 
        //    grid = grid
        //});
    }

    /*
     * L: Populates the grid[,] array with the given stiles.
     * This is what initially loads in the grid at the start of the scene if a grid is not already saved.
     */
    protected void SetGrid(STile[] stiles)
    {
        if (stiles.Length != Width * Height)
        {
            Debug.LogError("Only " + stiles.Length + " found when initializing grid! " +
                "Expected " + Width * Height + ".");
            return;
        }

        grid = new STile[Width, Height];
        foreach (STile t in stiles)
        {
            grid[t.x, t.y] = t;
        }
    }

    private void SetBGGrid(SGridBackground[] bgGridTiles) 
    {
        bgGrid = new SGridBackground[Width, Height];
        for (int i = 0; i < bgGridTiles.Length; i++)
        {
            bgGrid[i % Width, i / Width] = bgGridTiles[i];
        }
    }

    
    /// <summary>
    /// Returns a string like:   123_6##_4#5
    /// for a grid like:  1 2 3
    ///                   6 . .
    ///        (0, 0) ->  4 . 5
    /// </summary>
    /// <param name="numsOnly">If numsOnly is true, then the grid string will contain all tile IDs. If false, then inactive tiles will be represented by #s</param>
    /// <returns></returns>
    public static string GetGridString(bool numsOnly = false)
    {
        return GetGridString(Current.grid, numsOnly);
    }

    public static string GetGridString(STile[,] grid, bool numsOnly = false)
    {
        string s = "";
        for (int y = grid.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                if (numsOnly || grid[x, y].isTileActive)
                    s += Converter.IntToChar(grid[x, y].islandId);
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
        int[,] gridFormat = new int[Current.Width, Current.Height];
        for (int x = Current.Width - 1; x >= 0; x--)
        {
            for (int y = 0; y < Current.Height; y++)
            {
                gridFormat[y, (Current.Width - 1 - x)] = Converter.CharToInt(gridstring[(x * Current.Height) + y]);
            }
        }
        return gridFormat;
    }

    public STile GetStile(int islandId)
    {
        foreach (STile t in stiles)
            if (t.islandId == islandId)
                return t;
                
        return null;
    }

    public List<STile> GetStiles(List<int> idList)
    {
        List<STile> returnList = new List<STile>();
        foreach (STile t in stiles)
            if (idList.Contains(t.islandId))
                returnList.Add(t);
        return returnList;
    }

    protected static string GetTileIdAt(int x, int y)
    {
        return Current.targetGrid[(Current.Height - y - 1) * Current.Width + x].ToString();
    }

    public static STile GetTileAt(int x, int y)
    {
        return Current.grid[x,y];
    }

    protected void SwapTiles(STile one, STile two)
    {
        int x = two.x;
        int y = two.y;
        two.SetGridPosition(one.x, one.y);
        one.SetGridPosition(x, y);
        grid[two.x, two.y] = two;
        grid[x, y] = one;

        UIArtifact.SetButtonPos(one.islandId, x, y);
        UIArtifact.SetButtonPos(two.islandId, two.x, two.y);
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
                numCollected++;
        }
        return numCollected;
    }
    
    /// <summary>
    /// Returns the number of STiles available in the current SGrid.
    /// </summary>
    /// <returns></returns>
    public virtual int GetTotalNumTiles()
    {
        return Width * Height;
    }
    

    // DC: This will prefer an GameObjs parented STile if it has one
    public static STile GetSTileUnderneath(Transform entity, STile currentStileUnderneath)
    {
        STile[,] grid = SGrid.Current.GetGrid();
        float offset = grid[0, 0].STILE_WIDTH / 2f;

        STile stileUnderneath = null;
        foreach (STile s in grid)
        {
            if (s.isTileActive && PosInSTileBounds(entity.position, s.transform.position, offset))
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

    // C: result of consolidating 2 versions of this method
    // and i don't wanna rewrite method calls
    // DC: try to use the other one if possible
    public static STile GetStileUnderneath(GameObject target)
    {
        return GetSTileUnderneath(target.transform, target.GetComponentInParent<STile>());
    }

    public static bool PosInSTileBounds(Vector3 pos, Vector3 stilePos, float offset)
    {
        if (stilePos.x - offset < pos.x && pos.x < stilePos.x + offset &&
           (stilePos.y - offset < pos.y && pos.y < stilePos.y + offset ||
            stilePos.y - offset + Current.housingOffset < pos.y && pos.y < stilePos.y + offset + Current.housingOffset))
        {
            return true;
        }
        return false;
    }

    //C: Returns true if all tiles in the list are active, false otherwise
    public static bool AreTilesActive(List<STile> tiles)
    {
        foreach (STile s in tiles)
        {
            if(!s.isTileActive)
                return false;
        }
        return true;
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

    public List<Collectible> GetCollectibles()
    {
        List<Collectible> list = new List<Collectible>();
        foreach (Collectible c in collectibles)
            list.Add(c);
        return list;
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
            GetCollectible("Slider " + sliderId)?.gameObject.SetActive(true);
            AudioManager.Play("Puzzle Complete");
        }
    }

    public void GivePlayerTheCollectible(string name)
    {
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

        STile[,] newGrid = new STile[Width, Height];
        System.Array.Copy(grid, newGrid, Width * Height);
        foreach (Movement m in move.moves)
        {
            newGrid[m.endLoc.x, m.endLoc.y] = grid[m.startLoc.x, m.startLoc.y];
        }
        STile[,] oldGrid = grid;
        grid = newGrid;

        OnGridMove?.Invoke(this, new OnGridMoveArgs { oldGrid = oldGrid, grid = grid });
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
        stile.isTileCollected = true;
        UIArtifact.GetInstance().AddButton(stile, flickerButton);
        OnSTileEnabled?.Invoke(this, new OnSTileEnabledArgs { stile = stile });
    }

    public virtual void CollectStile(STile stile)
    {
        stile.isTileCollected = true;
        EnableStile(stile, true);
        OnSTileCollected?.Invoke(this, new OnSTileCollectedArgs { stile = stile });
    }

    public virtual bool TilesMoving()
    {
        List<STile> stiles = SGrid.Current.GetActiveTiles();
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
        Debug.Log($"[Saves] Saving data for {myArea}");
        SaveSystem.Current.SaveSGridData(myArea, this);
    }

    //L: Used in the save system to load a grid as opposed to using SetGrid(STile[], STile[]) with default tiles positions.
    public virtual void Load(SaveProfile profile) 
    { 
        // Default vars
        checkCompletion = false;

        //Debug.Log("Loading grid...");

        SGridData sgridData = profile.GetSGridData(myArea);

        if (sgridData.grid == null) {
            SetGrid(stiles);
            return;
        }

        Debug.Log($"[Saves] Loading saved data for {myArea}.");

        // setting grids... similar to initialization
        STile[,] newGrid = new STile[Width, Height];
        foreach (SGridData.STileData td in sgridData.grid) 
        {
            STile stile = GetStile(td.islandId); 
            newGrid[td.x, td.y] = stile;

            UIArtifact.SetButtonPos(td.islandId, td.x, td.y);
        }
        grid = newGrid;
        realigningGrid = sgridData.realigningGrid;

        // After updating grid, set the grids to active/not, calls events, etc.
        foreach (SGridData.STileData td in sgridData.grid)
        {
            STile stile = GetStile(td.islandId);
            stile.SetSTile(td.isTileActive, td.x, td.y);
        }
    }

    public virtual void SaveRealigningGrid()
    {
        //Debug.Log("Saved!");
        realigningGrid = new int[Current.Width, Current.Height];
        //GedGridString but turns it to SetGrid format
        for (int x = 0; x < Current.Width; x++)
        {
            for (int y = 0; y < Current.Height; y++)
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
        Current.SetGrid(GridStringToSetGridFormat(targetGrid));

        for (int x = 0; x < Current.Width; x++)
        {
            for (int y = 0; y < Current.Height; y++)
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
        Current.UpdateButtonCompletionsHelper();
    }

    protected virtual void UpdateButtonCompletionsHelper()
    {
        for (int x = 0; x < Current.Width; x++) {
            for (int y = 0; y < Current.Height; y++) {
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
        return Current.GetNumButtonCompletionsHelper();
    }

    protected virtual int GetNumButtonCompletionsHelper()
    {
        int numComplete = 0;
        for (int x = 0; x < Current.Width; x++) {
            for (int y = 0; y < Current.Height; y++) {
                string tids = GetTileIdAt(x, y);
                ArtifactTileButton artifactButton = UIArtifact.GetButton(x, y);
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
}