using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class STile : MonoBehaviour
{
    public bool isTileActive;
    public int islandId = -1;    //L: islandId is the id of the corresponding tile in the puzzle doc
    public int x = -1;
    public int y = -1;

    public bool hasAnchor;
    public STile linkTile; //C: is this still needed?

    private Vector2 movingDirection; // zero, right, up, left, down
    public class STileMoveArgs : System.EventArgs
    {
        public Vector2 moveDir;
    }
    public System.EventHandler<STileMoveArgs> onChangeMove; // called when STile starts moving or ends moving
    
    public int STILE_WIDTH = 17;

    private int sliderColliderDisableCount; // each enable gives this +1, disable does -1

    // Whether we have picked up this tile or not
    public bool isTileCollected;
    private int[] borderColliderDisableCount = new int[4];
    
    [Header("References")]
    public GameObject objects;
    public GameObject allTileMaps;
    public Collider2D sliderCollider;
    public Collider2D houseSliderCollider;
    public STileTilemap stileTilemaps;
    public STileTilemap houseTilemaps;
    private List<Collider2D> disabledColliders = new List<Collider2D>();
    private List<GameObject> disabledColliderTilemaps = new List<GameObject>();
    // these borders follow the tile and generally all activate/deactive together
    public GameObject[] borderColliders; // right top left bottom

    protected void Awake()
    {
        //L: Added so that Start is only called on objects within tiles that are enabled.
        SetTileActive(isTileActive);
    }

    protected void Start()
    {
        Init();
    }

    public virtual void Init()
    {
        // DC: this is so that we can call any other relevant functions when STiles are enabled in SGrid
        if (isTileActive) 
        {
            SGrid.Current.EnableStile(this, false);
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

    // Unused?
    public void SetSTile(STile other) 
    {
        isTileActive = other.isTileActive;
        isTileCollected = other.isTileCollected;
        x = other.x;
        y = other.y;
        Init();
    }

    public void SetSTile(bool isTileActive, int x, int y) 
    {
        this.isTileActive = isTileActive;
        this.x = x;
        this.y = y;
        Init();
    }

    public virtual void SetTileActive(bool isTileActive)
    {
        this.isTileActive = isTileActive;
        this.isTileCollected = isTileActive;

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
        stileTilemaps.colliders.gameObject.SetActive(sliderColliderDisableCount <= 0);
        houseTilemaps.colliders.gameObject.SetActive(sliderColliderDisableCount <= 0);
        
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
                        if (c.GetComponent<CompositeCollider2D>() != null 
                            && c.GetComponent<CompositeCollider2D>().isTrigger 
                            && c.CompareTag("LeaveTriggerEnabled"))
                            continue;
                        c.gameObject.SetActive(false);
                        disabledColliderTilemaps.Add(c.gameObject);
                        continue;

                    }

                    // do we need this here too?????
                    if (c.isTrigger && c.CompareTag("LeaveTriggerEnabled"))
                        continue;

                    // make lasers always hit even during movement
                    if (c.gameObject.layer == LayerMask.NameToLayer("LaserRaycast"))
                        continue;

                    c.enabled = false;
                    disabledColliders.Add(c);
                }
            }
            foreach (Collider2D c in objects.GetComponentsInChildren<Collider2D>())
            {
                if (c.isActiveAndEnabled)
                {
                    // maybe we should leave all triggers, but that could cause some npc/interacting weirdness
                    // in any case, we need to leave factory buttons enabled for sure
                    if (c.isTrigger && c.CompareTag("LeaveTriggerEnabled"))
                        continue;

                    // make lasers always hit even during movement
                    if (c.gameObject.layer == LayerMask.NameToLayer("LaserRaycast"))
                        continue;

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
                if (c != null)
                    c.enabled = true;
                else
                    Debug.LogWarning($"Tried to disable a collider in 'disabledColliders' but it was null!");
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
        return !hasAnchor;
    }

    public bool IsMoving()
    {
        return movingDirection != Vector2.zero;
    }

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

        transform.position = newPos;
        SetTileMapPositions(newPos);
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
        movingDirection = direction;
        onChangeMove?.Invoke(this, new STileMoveArgs {moveDir = movingDirection} );
    }

    public Vector2 GetMovingDirection()
    {
        return movingDirection;
    }

    public void SetMovingPosition(Vector2 position)
    {
        Vector3 newPos = calculateMovingPosition(position.x, position.y);
        transform.position = newPos;
        SetTileMapPositions(newPos);
    }

    protected void SetTileMapPositions(Vector3 pos)
    {
        pos = pos + new Vector3(-0.5f, -0.5f);

        allTileMaps.transform.position = pos;
    }

}