using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Minecart : Item, ISavable
{
    [Header("Movement")]
    [SerializeField] private float speed = 8.0f; 
    [SerializeField] private float cornerSpeed = 2.0f;
    
    private int currentDirection; //0 = East, 1 = North, 2 = West, 3 = South
    [SerializeField]    private int nextDirection;
    
    private float baseCornerSpeedMultiplier = 1; // cornerSpeed / speed
    private float cornerSpeedAmount = 1; // lerp between baseCornerSpeedMultiplier and 1

    private bool isOnTrack;
    [SerializeField] public bool isMoving {get; private set;} = false;

    public Vector3 offSet = new Vector3(0.5f, 0.75f, 0.0f);
    
    private bool canStartMoving = true;
    private List<GameObject> collidingObjects = new List<GameObject>();
    private bool collisionPause = false;

    [Header("Rail Managers")]
    private RailManager railManager;
    private RailManager borderRM; 

    [Header("Rail Tiles")]
    private RailTile currentTile;
    private RailTile targetTile;
    public Vector3Int currentTilePos; //position in tilemap grid space
    public Vector3Int targetTilePos; 
    public Vector3 prevWorldPos; //position in world space
    public Vector3 targetWorldPos;

    public LayerMask collidingMask;

    public STile currentSTile;

    private bool dropOnNextMove = false;


    [Header("State")]
    public MinecartState mcState;

    [Header("Animation")]
    [SerializeField] private float derailDuration;
    [SerializeField] private AnimationCurve xDerailMotion;
    [SerializeField] private AnimationCurve yDerailMotion;
    [SerializeField] private MinecartAnimationManager animator;

    private bool tileSwitch = false; //set to true when halfway between tiles, used for updating animation

    [Header("UI")]
    public Sprite trackerSpriteEmpty;
    public Sprite trackerSpriteRepair;
    public Sprite trackerSpriteLava;
    public Sprite trackerSpriteCrystal;

    private bool nextTile = false;

    public override void Awake() 
    {
        base.Awake();
        RailManager[] rms = FindObjectsOfType<RailManager>();
        foreach (RailManager r in rms) {
            if(r.isBorderRM)
                borderRM = r;
        }
        baseCornerSpeedMultiplier = cornerSpeed / speed;
        AddTracker();
    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
    }

    private void OnSTileMoveStart(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if(currentSTile == null || e.stile == null) return;
        if(e.stile == currentSTile && isMoving) Derail();
    }

    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if(mcState == MinecartState.Crystal) UpdateState("Empty");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(targetWorldPos, 0.2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetTilePos, 0.2f);
    }

    private void Update() 
    {
        if(Time.timeScale == 0) return;

        if(AllMovingConds())
        {
            Move();
        }
        else if (animator != null)
            animator.SetSpeed(0);
    }

    public override void SetLayer(int layer)
    {
        spriteRenderer.gameObject.layer = layer;
        animator.SetLayer(layer);
    }

    //TODO: Use raycasts to figure out if should be paused or not
    private void OnCollisionEnter2D(Collision2D other) 
    {
        collidingObjects.Add(other.gameObject);
        collisionPause = true;
    }

    private void OnCollisionExit2D(Collision2D other) 
    {
        collidingObjects.Remove(other.gameObject);
        if(collidingObjects.Count == 0)
            collisionPause = false;
    }

    #region  Movement

    private void Move()
    {
        animator.SetSpeed(1);
        float distToPrev = Vector3.Distance(transform.position, prevWorldPos);
        float distToNext = Vector3.Distance(transform.position, targetWorldPos);

        if(!tileSwitch && distToNext < distToPrev) UpdateAnimation();
        
        if(Vector3.Distance(transform.position, targetWorldPos) > 0.0001f)
        {
            nextTile = false;
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, Time.deltaTime * (cornerSpeedAmount * speed));
        }
        else if(!nextTile)
        {
            nextTile = true;
            transform.position = targetWorldPos;
            GetNextTile();
            tileSwitch = false;
        }
    }

    private bool AllMovingConds() => isMoving && isOnTrack && !collisionPause;

    private void UpdateAnimation()
    {
        if(currentDirection != nextDirection) {
            if(currentDirection == 2 || nextDirection == 2) {
                spriteRenderer.flipX = true;
                animator.FlipX(true);
                }
            PlayCurveAnimation();
        }
        else {
            cornerSpeedAmount = 1;
            spriteRenderer.flipX = false;
            animator.FlipX(false);
            PlayStraightAnimation();
        } 
        tileSwitch = true;
    }


    #endregion

    #region Item

    public override void PickUpItem(Transform pickLocation, System.Action callback = null)
    {
        base.PickUpItem(pickLocation, callback);
        VarManager.instance.SetBoolOn("MountainHasPickedUpMinecart");
        UITrackerManager.RemoveTracker(this.gameObject);
        animator.ChangeAnimationState("IDLE");
        if(mcState == MinecartState.Crystal || mcState == MinecartState.Lava)
            UpdateState("Empty");
    }

    public override STile DropItem(Vector3 dropLocation, System.Action callback=null) 
    {
        AddTracker();

        STile hitTile = SGrid.GetSTileUnderneath(gameObject);
        Tilemap railmap;

        if(hitTile != null)
        {
            railmap = hitTile.allTileMaps.GetComponentInChildren<STileTilemap>().minecartRails;
            railManager = railmap.GetComponent<RailManager>();
            
            DropMinecart(railmap, railManager, dropLocation, callback);

            UpdateParent();
            currentSTile = hitTile;
            return hitTile;
        }
        else if(borderRM) 
        {
            railManager = borderRM;
            railmap = borderRM.railMap;

            DropMinecart(railmap, railManager, dropLocation, callback);
            
            UpdateParentBorder();
            currentSTile = null;
        }
        else
            base.DropItem(dropLocation, callback);
        return null;
    }

    private void DropMinecart(Tilemap railmap, RailManager railManager, Vector3 dropLocation, System.Action callback)
    {
        if(railManager.railLocations.Contains(railmap.WorldToCell(dropLocation)))
        {
            StartCoroutine(AnimateDrop(railmap.CellToWorld(railmap.WorldToCell(dropLocation)) + offSet, callback));
            SnapToRail(railmap.WorldToCell(dropLocation));
        }
        else
            StartCoroutine(AnimateDrop(dropLocation, callback));
    }

    public override void OnEquip()
    {
        StopMoving();
        ResetTiles();
        currentSTile = null;
        tileSwitch = false;
    }

    #endregion


    #region movement

    public void StartMoving() 
    {
        if(isOnTrack && canStartMoving && !collisionPause)
        {
            isMoving = true; 
            animator.SetSpeed(1);
        } 
    }

    public void StopMoving(bool onTrack = false)
    {
        isMoving = false;
        if(!onTrack)
            isOnTrack = false;
        if (animator != null)
        {
            animator.SetSpeed(0);
        }
        collisionPause = false;
        collidingObjects.Clear();
    }

    public void ResetTiles()
    {
        currentTile = null;
        targetTile = null;
        currentTilePos = Vector3Int.zero;
        targetTilePos = Vector3Int.zero;
        tileSwitch = false;
    }

    public void SnapToRail(Vector3Int pos, int direction = -1)
    {
        transform.position = railManager.railMap.layoutGrid.CellToWorld(pos) + offSet;
        currentTile = railManager.railMap.GetTile(pos) as RailTile;
        currentTilePos = pos;
        prevWorldPos = railManager.railMap.layoutGrid.CellToWorld(currentTilePos) + offSet;
        currentDirection = direction == -1? currentTile.defaultDir: direction;
        if(railManager.railLocations.Contains(pos))
        {
            targetTilePos = currentTilePos + GetTileOffsetVector(currentDirection);
            targetTile = railManager.railMap.GetTile(targetTilePos) as RailTile;
            targetWorldPos = railManager.railMap.layoutGrid.CellToWorld(targetTilePos) + offSet;
            if(targetTile != null)
                nextDirection = GetDirection(targetTile, currentDirection);
            isOnTrack = true;
        }
        else
            ResetTiles();
    }

    public void SnapToRail(Vector3 pos, int dir = -1)
    {
        railManager = borderRM;
        Vector3Int newPos = railManager.railMap.WorldToCell(pos);
        SnapToRail(newPos, dir);
    }

    private void GetNextTile()
    {
        //Step 0: we reached the target tile. Set current tile/position to targets
        currentTile = targetTile;
        currentTilePos = targetTilePos;
        currentDirection = nextDirection;
        prevWorldPos = targetWorldPos;

        //Step 0.5: If we should drop now, try and drop
        if(dropOnNextMove)
        {
            if(TryDrop(true))
            {
                print("Drop wooo");
                dropOnNextMove = false;
                return;
            }
            else
            {
                print("you can no longer drop. this prob shouldn't happen");
                StopMoving();
                return;
            }
        }

        //Step 1: Try to go to tile in front in same rail manager
        if(TryGetNextTileSameRM()) { return;}

        //Step 2: if no tile exists at the next location on the same rail manager, try to find a rail manager with a tile in the desired spot
        if(TryGetNextTileDiffRM()) { return;}

        //Step 3: if neither of those work, check the drop location to see if there is a rail to drop down onto
        if(TryDrop()) {return;}

        //Step 4: if none of that works, stop
        StopMoving();
    }

    private bool TryGetNextTileSameRM()
    {
        Vector3Int target = currentTilePos + GetTileOffsetVector(currentDirection);
        if(railManager.railLocations.Contains(target))
        {
            targetTile = railManager.railMap.GetTile(target) as RailTile;
            int targetConnection = GetDirection(targetTile, currentDirection);
            if(targetConnection == -1)
            {
                Derail();
                return true;
            }
            targetTilePos = target;
            targetWorldPos = railManager.railMap.layoutGrid.CellToWorld(targetTilePos) + offSet;
            nextDirection = GetDirection(targetTile, currentDirection);
            return true;
        }
        return false;
    }

    private bool TryGetNextTileDiffRM()
    {
        List<STile> stileList = SGrid.Current.GetActiveTiles();
        List<RailManager> rmList = new List<RailManager>();
        Vector3Int targetLoc;
        foreach(STile tile in stileList)
        {
            RailManager[] otherRMs = tile.allTileMaps.GetComponentsInChildren<RailManager>();
            foreach(RailManager rm in otherRMs)
                if(rm != null && rm != railManager && rm.gameObject.activeSelf)
                    rmList.Add(rm);
        }
        
        Vector3Int target = currentTilePos + GetTileOffsetVector(currentDirection);
        foreach(RailManager rm in rmList) 
        {
            targetLoc = rm.railMap.layoutGrid.WorldToCell(railManager.railMap.layoutGrid.CellToWorld(target));
            if(rm.railLocations.Contains(targetLoc))
            {
                railManager = rm;
                SnapToRailNewSTile(targetLoc);
                UpdateParent();
                return true;
            }
        }

        if(borderRM)
        {
            targetLoc = borderRM.railMap.layoutGrid.WorldToCell(railManager.railMap.layoutGrid.CellToWorld(target));
            if(borderRM.railLocations.Contains(targetLoc))
            {
                railManager = borderRM;
                SnapToRailNewSTile(targetLoc);
                UpdateParentBorder();
                return true;
            }
        }
        return false;
    }

    public bool TryDrop(bool dropImmediate = false)
    {   
        STile tile = CheckDropTileBelow();
        bool canDrop = (dropImmediate || CheckFreeInFront()) && tile != null;
        if(canDrop)
        {
            if(dropImmediate) Drop(tile);
            else
            {
                targetWorldPos = prevWorldPos + GetTileOffsetVector(currentDirection);
                dropOnNextMove = true;
            }
        }
        return canDrop;
    }

    private bool CheckFreeInFront()
    {
        Vector3 checkLoc = prevWorldPos + GetTileOffsetVector(currentDirection);
        var colliders = Physics2D.OverlapBoxAll(checkLoc, Vector2.one * 0.5f, 0, collidingMask);
        bool valid = true;
        foreach(Collider2D c in colliders) {
            print(c.gameObject.name);
            if(c.GetComponent<STile>() == null && c.GetComponentInParent<Minecart>() == null )
            {
                valid = false;
            }
        }
        return valid;
    }

    private STile CheckDropTileBelow()
    {
        Vector3 checkLoc = prevWorldPos + (new Vector3Int(0,-1 * MountainGrid.Instance.layerOffset, 0));

        STile tile = null;
        var colliders = Physics2D.OverlapBoxAll(checkLoc, Vector2.one, 0);
        foreach(Collider2D c in colliders){
            if(c.GetComponent<STile>() != null && c.GetComponent<STile>().isTileActive) 
                tile = c.GetComponent<STile>();
        }
        return tile;

    }

    private void Drop(STile tile)
    {
        Vector3 checkLoc = transform.position + (new Vector3Int(0,-1 * MountainGrid.Instance.layerOffset, 0));
        transform.position = checkLoc;

        RailManager rm = tile.GetComponentInChildren<RailManager>();
        if(rm == null) 
        {
            Derail();
            return;
        }

        Vector3Int targetLoc = rm.railMap.layoutGrid.WorldToCell(checkLoc);
        if(!rm.railLocations.Contains(targetLoc))
        {
            Derail();
            return;
        }

        railManager = rm;
        SnapToRailNewSTile(targetLoc); 
        UpdateParent();
    }


    //Returns the outgoing direction from tile when entering from direction
    private int GetDirection(RailTile tile, int direction)
    {
        return tile.connections[(currentDirection + 2) % 4];
    }

    #endregion

    //Used to snap to a rail tile when moving across STiles
    public void SnapToRailNewSTile(Vector3Int pos)
    {
        currentTile = railManager.railMap.GetTile(pos) as RailTile;
        currentTilePos = pos;
        targetTilePos = currentTilePos + GetTileOffsetVector(currentDirection);
        targetTile = railManager.railMap.GetTile(targetTilePos) as RailTile;
        targetWorldPos = railManager.railMap.layoutGrid.CellToWorld(targetTilePos) + offSet; 
    }

    //makes the minecart fall off of the rails
    public void Derail()
    {
        StopMoving();
        ResetTiles();
        Vector3 derailVector = transform.position 
                               + speed * 0.3f * getDirectionAsVector(currentDirection)   
                               + 0.5f * (new Vector3(Random.onUnitSphere.x, Random.onUnitSphere.y, 0));
       // StartCoroutine(AnimateDerail(derailVector));
    }

    public void setCanStartMoving(bool canStart)
    {
        canStartMoving = canStart;
    }

    protected IEnumerator AnimateDerail(Vector3 target, System.Action callback = null)
    {
        float t = derailDuration;

        Vector3 start = new Vector3(transform.position.x, transform.position.y);
        while (t >= 0)
        {
            float x = xDerailMotion.Evaluate(t / derailDuration);
            float y = yDerailMotion.Evaluate(t / derailDuration);
            Vector3 pos = new Vector3(Mathf.Lerp(target.x, start.x, x),
                                      Mathf.Lerp(target.y, start.y, y));
            
            base.spriteRenderer.transform.position = pos;
            yield return null;
            t -= Time.deltaTime;
        }

        transform.position = target;
        base.spriteRenderer.transform.position = target;
        callback();

    }

    #region State

    public void UpdateState(string stateName){
        if(stateName.Equals("Player"))
            mcState = MinecartState.Player;
        else if(stateName.Equals("Lava"))
            mcState = MinecartState.Lava;
        else if(stateName.Equals("Crystal"))
            mcState = MinecartState.Crystal;
        else if (stateName.Equals("Empty"))
            mcState = MinecartState.Empty;
        else if (stateName.Equals("RepairParts"))
            mcState = MinecartState.RepairParts;
        else
            Debug.LogWarning("Invalid Minecart State. Should be Player, Lava, Empty, RepairParts, or Crystal");
        UpdateIcon();
        UpdateContentsSprite();
    }

    public bool TryAddCrystals()
    {
        if(mcState == MinecartState.Empty)
        {
            UpdateState("Crystal");
            return true;
        }
        return false;
    }

    #endregion


    #region UI

    private void UpdateIcon()
    {
        UITrackerManager.RemoveTracker(this.gameObject);
        AddTracker();
    }

    private void AddTracker()
    {
        if(mcState == MinecartState.Lava)
            UITrackerManager.AddNewTracker(this.gameObject, trackerSpriteLava);
        else if(mcState == MinecartState.Crystal)
            UITrackerManager.AddNewTracker(this.gameObject, trackerSpriteCrystal);
        else if(mcState == MinecartState.Empty)
            UITrackerManager.AddNewTracker(this.gameObject, trackerSpriteEmpty);
        else if(mcState == MinecartState.RepairParts)
            UITrackerManager.AddNewTracker(this.gameObject, trackerSpriteRepair);
    }

    #endregion
    
    #region Animation

    // Called during animations as an event
    public void SetCornerPercent(float percent)
    {
        percent = Mathf.Clamp01(percent);
        cornerSpeedAmount = Mathf.Lerp(baseCornerSpeedMultiplier, 1, percent);
    }

    private void UpdateContentsSprite()
    {
        animator.ChangeContents(mcState);
    }

    private void PlayCurveAnimation()
    {
        //magic number based on enum order, better than 8 case switch
        // dc: no way :sob:
        int animationNum = 5 + (currentDirection * 2) + (nextDirection / 2);
        animator.ChangeAnimationState(animationNum);
    }
    
    private void PlayStraightAnimation()
    {
        animator.ChangeAnimationState(currentDirection + 1);
    }

    private void PlayStoppedAnimation()
    {

    }

    


    #endregion


    #region Utility

    private void UpdateParent()
    {
        gameObject.transform.parent = railManager.gameObject.GetComponentInParent<STile>().transform.Find("Objects").transform;
        currentSTile = railManager.gameObject.GetComponentInParent<STile>();
    }

    private void UpdateParentBorder()
    {
        gameObject.transform.parent = borderRM.gameObject.GetComponentInParent<STileTilemap>().transform.Find("Objects").transform;
        currentSTile = null;
    }

    private void UpdateParent(STile sTile)
    {
        gameObject.transform.parent = sTile.transform.Find("Objects").transform;
        currentSTile = sTile;
    }

    //C: returns a vector that can be added to the tile position in order to determine the location of the specified point
    public static Vector3Int GetTileOffsetVector(int num)
    {
        int[] arr = {1, 0, -1, 0};
        return new Vector3Int(arr[num], arr[(num+3) % 4], 0);
    }

    private static Vector3 getDirectionAsVector(int dir)
    {
        Vector3[] arr = {Vector3.right, Vector3.up, Vector3.left, Vector3.down};
        return arr[dir];
    }

    #endregion


    #region Conditionals

    public void CheckIsEmpty(Condition c){
        c.SetSpec(mcState == MinecartState.Empty);
    }

    public void CheckCrystals(Condition c){
        c.SetSpec(mcState == MinecartState.Crystal);
    }

    public void CheckIsMoving(Condition c){
        c.SetSpec(isMoving);
    }

    public void CheckIsNotMoving(Condition c){
        c.SetSpec(!isMoving);
    }

    #endregion


    #region save/load

    public override void Save()
    {
        base.Save();

        SaveSystem.Current.SetInt("mountainMCState", (int)mcState);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);

        int state = profile.GetInt("mountainMCState");
        mcState = (MinecartState)state;
        UpdateIcon();
    }

    #endregion
}
