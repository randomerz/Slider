using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Minecart : Item, ISavable
{
    [Header("Movement")]
    [SerializeField] private float speed = 8.0f; 
    [SerializeField] private float cornerSpeed = 2.0f;
    
    public int currentDirection; //0 = East, 1 = North, 2 = West, 3 = South
    [SerializeField]    private int nextDirection;
    
    private float baseCornerSpeedMultiplier = 1; // cornerSpeed / speed
    private float cornerSpeedAmount = 1; // lerp between baseCornerSpeedMultiplier and 1

    public bool isOnTrack;
    [SerializeField] public bool isMoving {get; private set;} = false;

    public Vector3 offSet = new Vector3(0.5f, 0.75f, 0.0f);
    
    private bool canStartMoving = true;
    private List<GameObject> collidingObjects = new List<GameObject>();
    private bool collisionPause = false;

    [Header("Rail Managers")]
    private RailManager railManager;
    public RailManager borderRM; 

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

    private int numberOfPickups = 0;
    public int NumPickups => numberOfPickups;

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
    public Sprite trackerSpriteLava;
    public Sprite trackerSpriteCrystal;

    private bool nextTile = false;
    public LayerMask blocksSpawnMask;



    public override void Awake() 
    {
        base.Awake();
        baseCornerSpeedMultiplier = cornerSpeed / speed;
        AddTracker();
    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
        ArtifactTabManager.AfterScrollRearrage += OnScrollRearrange;
    }


    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= OnSTileMoveStart;
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
        ArtifactTabManager.AfterScrollRearrage -= OnScrollRearrange;
    }

    private void OnSTileMoveStart(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if(currentSTile == null || e.stile == null) return;
        if(e.stile == currentSTile && isMoving) Derail();
    }

    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if(mcState == MinecartState.Crystal) UpdateState(MinecartState.Empty);
    }

    private void OnScrollRearrange(object sender, EventArgs e)
    {
        if(isMoving) Derail();
        if(mcState == MinecartState.Crystal) UpdateState(MinecartState.Empty);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(targetWorldPos, 0.2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetTilePos, 0.2f);
    }

    public override void Update() 
    {
        base.Update();

        if (Time.timeScale == 0) return;

        if (AllMovingConds())
        {
            Move();
        }
        else if (animator != null)
        {
            animator.SetSpeed(0);
        }
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
        numberOfPickups++;
        VarManager.instance.SetBoolOn("MountainHasPickedUpMinecart");
        UITrackerManager.RemoveTracker(gameObject);
        animator.ChangeAnimationState("IDLE");
        if(mcState == MinecartState.Crystal || mcState == MinecartState.Lava)
            UpdateState(MinecartState.Empty, addTracker: false);
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

    private void DropMinecart(Tilemap railmap, RailManager railManager, Vector3 dropLocation, System.Action callback, int count = 0)
    {
        if(count > 2)
        {
            Debug.LogError("infinite recursion in minecart drop. This should never happen!");
            StartCoroutine(AnimateDrop(dropLocation, callback));
            return;
        }
        if(railManager.railLocations.Contains(railmap.WorldToCell(dropLocation)))
        {
            bool snap;
            Vector3 loc = GetDropLocation(dropLocation, railmap, out snap);
            StartCoroutine(AnimateDrop(loc, callback));
            if(snap)
                SnapToRail(railmap.WorldToCell(dropLocation));
        }
        else if(railManager.otherRMOnTile != null 
                && railManager.otherRMOnTile.railLocations.Contains(railmap.WorldToCell(dropLocation)))
        {
            this.railManager = railManager.otherRMOnTile;
            DropMinecart(railManager.railMap, railManager.otherRMOnTile, dropLocation, callback, count + 1);
        }
        else
            StartCoroutine(AnimateDrop(dropLocation, callback));
    }
    
    private Vector3 GetDropLocation(Vector3 dropLocation, Tilemap railmap, out bool snap)
    {
        snap = false;
        Vector3 targetSnapLoc = railmap.CellToWorld(railmap.WorldToCell(dropLocation)) + offSet;
        LayerMask mask = LayerMask.GetMask(new string[]{"Item"});
        if(Physics2D.OverlapCircleAll(targetSnapLoc, 1, mask).Count() == 0)
        {
            snap = true;
            return railmap.CellToWorld(railmap.WorldToCell(dropLocation)) + offSet;
        }
        return dropLocation;
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
        if(currentTile == null)
            print("current tile null");
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
                dropOnNextMove = false;
                return;
            }
            else
            {
                Debug.LogWarning("minecart can no longer drop. this prob shouldn't happen");
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
        bool canDrop = tile != null && CheckFreeInFrontAndBelow();
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

    private bool CheckFreeInFrontAndBelow()
    {
        //Only the big Ice patch allows for drops now. Must be vertical on tile 4.
        if(currentSTile == null || currentSTile.islandId != 4 || currentDirection % 2 == 0 || transform.localPosition.y > 7) 
        {
            return false;
        }

        //check colliders below
        Vector3 checkloc = prevWorldPos + (new Vector3Int(0,-1 * MountainGrid.Instance.layerOffset, 0)) + GetTileOffsetVector(currentDirection);
        Vector3 loc = ItemPlacerSolver.FindItemPlacePosition(checkloc, 0, blocksSpawnMask, true);
        if( loc.x == float.MaxValue)
        {
            return false;
        }
        return true;
    }

    private STile CheckDropTileBelow()
    {
        Vector3 checkloc = prevWorldPos + (new Vector3Int(0,-1 * MountainGrid.Instance.layerOffset, 0)) + GetTileOffsetVector(currentDirection);
        STile tile = null;
        var colliders = Physics2D.OverlapBoxAll(checkloc, Vector2.one * 0.4f, 0);
        foreach(Collider2D c in colliders){
            STile t = c.GetComponent<STile>();
            if(t != null && t.isTileActive && !t.IsMoving()) 
            {
                tile = t;
            }
        }
        return tile;

    }

    private void Drop(STile tile)
    {
        AudioManager.Play("Fall");
        Vector3 checkLoc = transform.position + (new Vector3Int(0,-1 * MountainGrid.Instance.layerOffset, 0));
        transform.position = checkLoc;

        RailManager rm = tile.GetComponentInChildren<RailManager>();
        if(rm == null) 
        {
            Derail();
            UpdateParent();
            return;
        }

        Vector3Int targetLoc = rm.railMap.layoutGrid.WorldToCell(checkLoc);
        if(!rm.railLocations.Contains(targetLoc))
        {
            Derail();
            UpdateParent();
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

    public void UpdateState(MinecartState state, bool addTracker = true){
        mcState = state;
        if(addTracker)
            UpdateIcon();
        UpdateContentsSprite();
    }

    public bool TryAddCrystals()
    {
        if(mcState != MinecartState.Crystal)
        {
            UpdateState(MinecartState.Crystal); 
            return true;
        }
        return false;
    }

    #endregion


    #region UI

    private void UpdateIcon()
    {
        UITrackerManager.RemoveTracker(gameObject);
        AddTracker();
    }

    private void AddTracker()
    {
        if(mcState == MinecartState.Lava)
            UITrackerManager.AddNewTracker(gameObject, trackerSpriteLava);
        else if(mcState == MinecartState.Crystal)
            UITrackerManager.AddNewTracker(gameObject, trackerSpriteCrystal);
        else if(mcState == MinecartState.Empty)
            UITrackerManager.AddNewTracker(gameObject, trackerSpriteEmpty);
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
        //magic number based on enum order
        int animationNum = 5 + (currentDirection * 2) + (nextDirection / 2);
        animator.ChangeAnimationState(animationNum);
    }
    
    private void PlayStraightAnimation()
    {
        animator.ChangeAnimationState(currentDirection + 1);
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
        SaveSystem.Current.SetInt("mountainMCNumPickups", numberOfPickups);
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        numberOfPickups = SaveSystem.Current.GetInt("mountainMCNumPickups");
    }

    #endregion
}
