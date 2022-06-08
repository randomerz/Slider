using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Minecart : Item
{
    
    [SerializeField] private int currentDirection;
    public RailManager railManager;
    [SerializeField] private bool isOnTrack;
    [SerializeField] private bool isMoving;
    private bool canStartMoving = true;
    [SerializeField] public RailTile currentTile;
    [SerializeField] public RailTile targetTile;
    [SerializeField] private float speed = 2.0f;
    public Vector3 offSet = new Vector3(0.5f, 0.5f, 0.0f);
    [SerializeField] private RailManager borderRM;
    private STile currentSTile;


    public Vector3Int currentTilePos;
    public Vector3Int targetTilePos; 
    public Vector3 targetWorldPos;

    [SerializeField] private float derailDuration;
    [SerializeField] private AnimationCurve xDerailMotion;
    [SerializeField] private AnimationCurve yDerailMotion;

    
    public Sprite trackerSprite;
    [SerializeField] private Animator minecartAnimator;


    public void setCanStartMoving(bool canStart)
    {
        canStartMoving = canStart;
    }

    //C: this is horrible
    private void Awake() 
    {
        RailManager[] rms = FindObjectsOfType<RailManager>();
        foreach (RailManager r in rms) {
            if(r.isBorderRM)
                borderRM = r;
        }
        UITrackerManager.AddNewTracker(gameObject, trackerSprite);
        minecartAnimator ??= GetComponent<Animator>();

    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += OnMoveStart;
        SGridAnimator.OnSTileMoveEnd += OnMoveEnd;

    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= OnMoveStart;
        SGridAnimator.OnSTileMoveEnd += OnMoveEnd;
    }

    private void OnMoveStart(object sender, SGridAnimator.OnTileMoveArgs tileMoveArgs)
    {
        if(tileMoveArgs.stile = currentSTile)
        {
            if(isMoving)
                Derail();
            else
                canStartMoving = false;
        }
    }

    private void OnMoveEnd(object sender, SGridAnimator.OnTileMoveArgs tileMoveArgs)
    {
        if(tileMoveArgs.stile = currentSTile)
            canStartMoving = true;
        //recalculate target position
    }

    public override void PickUpItem(Transform pickLocation, System.Action callback = null)
    {
        base.PickUpItem(pickLocation, callback);
        UITrackerManager.RemoveTracker(this.gameObject);
    }

    //Item Related Stuff
    public override STile DropItem(Vector3 dropLocation, System.Action callback=null) 
    {
        UITrackerManager.AddNewTracker(this.gameObject, trackerSprite);
        Collider2D hit = Physics2D.OverlapPoint(dropLocation, LayerMask.GetMask("Slider"));
        if (hit == null)
            return null;
        
        if(hit.GetComponent<STile>()) //Use Stile RM
        {
            STile hitTile = hit.GetComponent<STile>();
            Tilemap railmap = hitTile.allTileMaps.GetComponentInChildren<STileTilemap>().minecartRails;
            railManager = railmap.GetComponent<RailManager>();
            if(railManager.railLocations.Contains(railmap.WorldToCell(dropLocation))) //C: If this is dropped onto rails, it will snap into position (change to station only later?)
            {
                StartCoroutine(AnimateDrop(railmap.CellToWorld(railmap.WorldToCell(dropLocation)) + offSet, callback));
                SnapToRail(railmap.WorldToCell(dropLocation));
            }
            else
                StartCoroutine(AnimateDrop(dropLocation, callback));
            
            gameObject.transform.parent = hitTile.transform.Find("Objects").transform;
            currentSTile = hitTile;
            return hitTile;
        }
        else if(hit.GetComponent<STileTilemap>()) //use border RM
        {
            Tilemap railmap = hit.GetComponent<STileTilemap>().minecartRails;
            railManager = railmap.GetComponent<RailManager>();

            if(railManager.railLocations.Contains(railmap.WorldToCell(dropLocation))) //C: If this is dropped onto rails, it will snap into position (change to station only later?)
            {
                StartCoroutine(AnimateDrop(railmap.CellToWorld(railmap.WorldToCell(dropLocation)) + offSet, callback));
                SnapToRail(railmap.WorldToCell(dropLocation));
            }
            else
                StartCoroutine(AnimateDrop(dropLocation, callback));
            //decouple from RM
            gameObject.transform.parent = borderRM.gameObject.GetComponentInParent<STileTilemap>().transform.Find("Objects").transform;
        }
        return null;

    }

    public override void OnEquip()
    {
        StopMoving();
        ResetTiles();
        currentSTile = null;
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        Debug.Log("bonk");
        StopMoving(true);
    }




    public void StartMoving() 
    {
        if(isOnTrack && canStartMoving)
        {
            isMoving = true; 
            minecartAnimator.SetBool("isMoving", true);
        } 
    }

    public void StopMoving(bool onTrack = false)
    {
        isMoving = false;
        isOnTrack = onTrack? isOnTrack: false;
        minecartAnimator.SetBool("isMoving", false);
    }

    public void ResetTiles()
    {
        currentTile = null;
        targetTile = null;
        currentTilePos = Vector3Int.zero;
        targetTilePos = Vector3Int.zero;
    }

    //Places the minecart on the tile at the given position
    public void SnapToRail(Vector3Int pos)
    {
        transform.position = railManager.railMap.layoutGrid.CellToWorld(pos) + offSet;
        currentTile = railManager.railMap.GetTile(pos) as RailTile;
        currentTilePos = pos;
        currentDirection = currentTile.defaultDir;
        if(railManager.railLocations.Contains(pos))
        {
            targetTilePos = currentTilePos + GetTileOffsetVector(currentDirection);
            targetTile = railManager.railMap.GetTile(targetTilePos) as RailTile;
            targetWorldPos = railManager.railMap.layoutGrid.CellToWorld(targetTilePos) + offSet;
            isOnTrack = true;
        }
        else
            ResetTiles();
    }

    

    private void Update() 
    {
        if(Time.timeScale == 0) return;
        if(isMoving && isOnTrack)
        {
            if(Vector3.Distance(transform.position, targetWorldPos) < 0.01f)
                GetNextTile();
            else
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, Time.deltaTime * speed);
        }
    }

    private void GetNextTile()
    {
        currentTile = targetTile;
        currentTilePos = targetTilePos;
        targetTilePos = currentTilePos + GetTileOffsetVector(currentDirection);
        if(railManager.railLocations.Contains(targetTilePos))
        {
            targetTile = railManager.railMap.GetTile(targetTilePos) as RailTile;
            int targetConnection = targetTile.connections[(currentDirection + 2) % 4];
            if(targetConnection == -1) //this is a broken track, derail
            {
                Derail();
                return;
            }    

            targetWorldPos = railManager.railMap.layoutGrid.CellToWorld(targetTilePos) + offSet;
            currentDirection = targetTile.connections[(currentDirection + 2) % 4];
        }
        else
        {
            LookForRailManager();
        }
        
    }

    /*C: looks for a rail manager that has a tile which overlaps with the target position
     * If one is found, rail manager is updated so the minecart can operate on the new STile
     * If one is not found, the minecart derails
     */
    private void LookForRailManager()
    {
        List<STile> stileList = SGrid.current.GetActiveTiles();
        List<RailManager> rmList = new List<RailManager>();
        Vector3Int targetLoc;
        foreach(STile tile in stileList)
        {
            RailManager otherRM = tile.allTileMaps.GetComponentInChildren<RailManager>();
                if(otherRM != null)
                    rmList.Add(otherRM);
            }
            foreach(RailManager rm in rmList) //look and see if the next location overlaps with a location of a rail on another STile
            {
                targetLoc = rm.railMap.layoutGrid.WorldToCell(railManager.railMap.layoutGrid.CellToWorld(targetTilePos));
                if(rm.railLocations.Contains(targetLoc))
                {
                    railManager = rm;
                    SnapToRailNewSTile(targetLoc);
                    gameObject.transform.parent = rm.gameObject.GetComponentInParent<STile>().transform.Find("Objects").transform;
                    return;
                }
                //C: Check if the minecart can drop down to the tile below
                else if(MountainGrid.instance && rm.railLocations.Contains(targetLoc + new Vector3Int(0,-1 * MountainGrid.instance.layerOffset, 0)))
                {
                    railManager = rm;
                    transform.position += new Vector3Int(0,-1 * MountainGrid.instance.layerOffset, 0);
                    SnapToRailNewSTile(targetLoc + new Vector3Int(0,-1 * MountainGrid.instance.layerOffset, 0)); //give seperate method for between layers?
                    gameObject.transform.parent = rm.gameObject.GetComponentInParent<STile>().transform.Find("Objects").transform;
                    return;
                }
            }
            if(borderRM) {
            targetLoc = borderRM.railMap.layoutGrid.WorldToCell(railManager.railMap.layoutGrid.CellToWorld(targetTilePos));
            if(borderRM.railLocations.Contains(targetLoc)) //look and see if the next location overlaps with a location of a rail on the outside
            {
                railManager = borderRM;
                SnapToRailNewSTile(targetLoc);
                gameObject.transform.parent = borderRM.gameObject.GetComponentInParent<STileTilemap>().transform.Find("Objects").transform;
                return;
            }
            }
        StopMoving();
    }


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

    //returns a vector that can be added to the tile position in order to determine the location of the specified point
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
}
