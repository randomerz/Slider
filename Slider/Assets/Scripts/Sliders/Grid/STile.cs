using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class STile : MonoBehaviour
{
    public bool isTileActive;
    public int islandId = -1;
    public int x = -1;
    public int y = -1;

    public bool hasAnchor;
    public STile linkTile; // Probably should be a list, set in instpector

    private Vector2 movingDirection; // zero, right, up, left, down
    public class STileMoveArgs : System.EventArgs
    {
        public Vector2 moveDir;
    }
    public System.EventHandler<STileMoveArgs> onChangeMove; // called when STile starts moving or ends moving
    
    public int STILE_WIDTH = 17;

    private int sliderColliderDisableCount; // each enable gives this +1, disable does -1

    // Whether we have picked up this tile or not. Used in MagiTech so that only collected tiles
    // are enabled when swapping grids.
    public bool isTileCollected;
    private int[] borderColliderDisableCount = new int[4];
    
    [Header("References")]
    public GameObject objects;
    public GameObject allTileMaps;
    public Collider2D sliderCollider;
    public Collider2D houseSliderCollider;
    public GameObject tileMapCollider;
    private List<Collider2D> disabledColliders = new List<Collider2D>();
    private List<GameObject> disabledColliderTilemaps = new List<GameObject>();
    // these borders follow the tile and generally all activate/deactive together
    public GameObject[] borderColliders; // right top left bottom

    protected void Start()
    {
        Init();
        // Debug.Log(STILE_WIDTH);
    }

    public virtual void Init()
    {
        // SetTileActive(isTileActive);
        // DC: this is so that we can call any other relevant functions when STiles are enabled in SGrid
        if (isTileActive) 
        {
            SGrid.current.EnableStile(this, false);
        }
        else
        {
            SetTileActive(isTileActive); 
        }

        Vector3 defaultPos = calculatePosition(x,y);
        transform.position = defaultPos;
        SetTileMapPositions(defaultPos);
        sliderColliderDisableCount = 0;
    }
    

    // Update is called once per frame
    protected void Update()
    {
        
    }


    public void SetSTile(STile other) {
        isTileActive = other.isTileActive;
        x = other.x;
        y = other.y;
        Init();
    }

    public void SetSTile(bool isTileActive, int x, int y) {
        this.isTileActive = isTileActive;
        this.x = x;
        this.y = y;
        Init();
    }

    public virtual void SetTileActive(bool isTileActive)
    {
        this.isTileActive = isTileActive;

        objects.SetActive(isTileActive);
        allTileMaps.SetActive(isTileActive);

        sliderCollider.isTrigger = isTileActive;
        houseSliderCollider.isTrigger = isTileActive;
    }

    // when sliderColliderDisableCount > 0, its disabled
    public bool SetSliderCollider(bool isActive)
    {
        if (!isActive)
            sliderColliderDisableCount += 1;
        else
            sliderColliderDisableCount -= 1;

        sliderCollider.enabled = sliderColliderDisableCount <= 0;
        houseSliderCollider.enabled = sliderColliderDisableCount <= 0;
        tileMapCollider.SetActive(sliderColliderDisableCount <= 0);
        
        if (sliderColliderDisableCount > 0)
        {
            // disable internal colliders (decorations, npcs, etc.)
            foreach (Collider2D c in allTileMaps.GetComponentsInChildren<Collider2D>())
            {
                if (c.isActiveAndEnabled)
                {
                    if (c is TilemapCollider2D)
                    {
                        // DC: disabling these is weird because they have a 2d tilemap collider and a composite collider?
                        // Debug.LogWarning("Found a tilemap " + name + ", " + c.name + " while moving tiles! This may result in unexpected behavior.");

                        // skip triggers because they kinda wacky?
                        // if (c.isTrigger)
                        c.gameObject.SetActive(false);
                        disabledColliderTilemaps.Add(c.gameObject);
                        continue;

                    }
                    c.enabled = false;
                    disabledColliders.Add(c);
                }
            }
            foreach (Collider2D c in objects.GetComponentsInChildren<Collider2D>())
            {
                if (c.isActiveAndEnabled)
                {
                    c.enabled = false;
                    disabledColliders.Add(c);
                }
            }
        }
        else
        {
            // enable internal colliders
            foreach (Collider2D c in disabledColliders)
            {
                c.enabled = true;
            }
            disabledColliders.Clear();
            foreach (GameObject g in disabledColliderTilemaps)
            {
                g.SetActive(true);
            }
            disabledColliderTilemaps.Clear();
        }

        return sliderColliderDisableCount <= 0;
    }

    public void SetBorderCollider(int index, bool isActive)
    {
        // borderColliders[index].SetActive(isActive);
        if (isActive)
            borderColliderDisableCount[index] += 1;
        else
            borderColliderDisableCount[index] = Mathf.Max(0, borderColliderDisableCount[index] - 1);
        borderColliders[index].SetActive(borderColliderDisableCount[index] > 0);
    }

    public void SetBorderColliders(bool isActive)
    {
        for (int i = 0; i < borderColliders.Length; i++)
        {
            SetBorderCollider(i, isActive);
        }
    }

    public bool CanMove(int x, int y)
    {
        if (hasAnchor)
            return false;

        return true;
    }

    // CanRotate() => no anchor and not linked

    public void SetGridPosition(Vector2Int v)
    {
        SetGridPosition(v.x, v.y);
    }

    // Use this one usually!
    public virtual void SetGridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        Vector3 newPos = calculatePosition(x, y);

        //StartCoroutine(StartCameraShakeEffect());

        if (isTileActive)
        {
            // animations and style => physics on tile
            Vector3 dr = newPos - transform.position;
            UpdateTilePhysics(dr);

            transform.position = newPos;
            SetTileMapPositions(newPos);
        }
        else
        {
            transform.position = newPos;
            SetTileMapPositions(newPos);
        }
    }

    public virtual Vector3 calculatePosition(int x, int y) 
    {
        return STILE_WIDTH * new Vector3(x, y);
    }

    public virtual Vector3 calculateMovingPosition(float x, float y) 
    {
        return STILE_WIDTH * new Vector3(x, y);
    }

    public virtual void SetGridPositionRaw(int x, int y)
    {
        this.x = x;
        this.y = y;
        Vector3 newPos = calculatePosition(x, y);
        transform.position = newPos;
        SetTileMapPositions(newPos);
    }



    public void SetMovingDirection(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            // stop moving
            movingDirection = direction;
        }
        else
        {
            // start moving
            movingDirection = direction;
        }

        onChangeMove?.Invoke(this, new STileMoveArgs {moveDir = movingDirection} );
    }

    public Vector2 GetMovingDirection()
    {
        return movingDirection;
    }

    public void SetMovingPosition(Vector2 position)
    {
        Vector3 newPos = calculateMovingPosition(position.x, position.y);
        // physics
        Vector3 dr = newPos - transform.position;
        UpdateTilePhysics(dr);


        transform.position = newPos;
        SetTileMapPositions(newPos);
    }

    protected void UpdateTilePhysics(Vector3 dr)
    {
        // if player is on stile, move them
        //              THIS IS TEMPORARY, REPLACE WITH PROPPER CHECK ON ALL SLIDEABLES
        // int playerIsland = Player.GetStileUnderneath().islandId;
        // if (playerIsland == islandId)
        // {
        //     Player.SetPosition(Player.GetPosition() + dr);
        // }
    }


    protected void SetTileMapPositions(Vector3 pos)
    {
        pos = pos + new Vector3(-0.5f, -0.5f);

        allTileMaps.transform.position = pos;
    }

    // DC: a better way of calculating which stile the player is on, accounting for overlapping stiles
    public static STile GetSTileUnderneath(Transform entity, STile prevUnderneath)
    {
        // this doesnt work when you queue a move and stand at the edge. for some reason, on the moment of impact hits does not overlap with anything??
        // Collider2D[] hits = Physics2D.OverlapPointAll(_instance.transform.position, LayerMask.GetMask("Slider"));
        // Debug.Log("Hit " + hits.Length + " at " + _instance.transform.position);

        // STile stileUnderneath = null;
        // for (int i = 0; i < hits.Length; i++)
        // {
        //     STile s = hits[i].GetComponent<STile>();
        //     if (s != null && s.isTileActive)
        //     {
        //         if (currentStileUnderneath != null && s.islandId == currentStileUnderneath.islandId)
        //         {
        //             // we are still on top of the same one
        //             return;
        //         }
        //         if (stileUnderneath == null)
        //         {
        //             // otherwise we only care about the first hit
        //             stileUnderneath = s;
        //         }
        //     }
        // }
        // currentStileUnderneath = stileUnderneath;

        STile[,] grid = SGrid.current.GetGrid();
        float offset = grid[0, 0].STILE_WIDTH / 2f;
        float housingOffset = -150;

        //C: The housing offset in the mountain is -250 due to the map's large size
        if (SGrid.current is MountainGrid)
            housingOffset -= 100;

        STile stileUnderneath = null;
        foreach (STile s in grid)
        {
            if (s.isTileActive && PosInSTileBounds(entity.position, s.transform.position, offset, housingOffset))
            {
                if (prevUnderneath != null && s.islandId == prevUnderneath.islandId)
                {
                    // we are still on top of the same one
                    return prevUnderneath;
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

    private static bool PosInSTileBounds(Vector3 pos, Vector3 stilePos, float offset, float housingOffset)
    {
        if (stilePos.x - offset < pos.x && pos.x < stilePos.x + offset &&
           (stilePos.y - offset < pos.y && pos.y < stilePos.y + offset ||
            stilePos.y - offset + housingOffset < pos.y && pos.y < stilePos.y + offset + housingOffset))
        {
            return true;
        }
        return false;
    }
}
