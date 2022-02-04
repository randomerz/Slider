using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STile : MonoBehaviour
{
    public bool isTileActive;
    public int islandId = -1;
    public int x = -1;
    public int y = -1;

    public bool hasAnchor;
    public STile linkTile; // Probably should be a list, set in instpector

    private Vector2 movingDirection; // zero, right, up, left, down
    
    public int STILE_WIDTH = 17;

    private int sliderColliderDisableCount; // each enable gives this +1, disable does -1
    
    [Header("References")]
    public GameObject objects;
    public Collider2D sliderCollider;
    public Collider2D houseSliderCollider;
    public GameObject tileMapCollider;
    // these borders follow the tile and generally all activate/deactive together
    public GameObject[] borderColliders; // right top left bottom
    public GameObject stileTileMaps;

    protected void Awake()
    {
        Init();
        // Debug.Log(STILE_WIDTH);
    }

    public void Init()
    {
        SetTileActive(isTileActive);
        Vector3 defaultPos = STILE_WIDTH * new Vector3(x, y);
        transform.position = defaultPos;
        SetTileMapPositions(defaultPos);
        sliderColliderDisableCount = 0;
    }
    

    // Update is called once per frame
    void Update()
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

    public void SetTileActive(bool isTileActive)
    {
        this.isTileActive = isTileActive;

        objects.SetActive(isTileActive);
        stileTileMaps.SetActive(isTileActive);

        sliderCollider.isTrigger = isTileActive;
        houseSliderCollider.isTrigger = isTileActive;
    }

    // when sliderColliderDisableCount > 0, its 
    public bool SetSliderCollider(bool isActive)
    {
        if (!isActive)
            sliderColliderDisableCount += 1;
        else
            sliderColliderDisableCount -= 1;

        sliderCollider.enabled = sliderColliderDisableCount <= 0;
        houseSliderCollider.enabled = sliderColliderDisableCount <= 0;
        tileMapCollider.SetActive(sliderColliderDisableCount <= 0);

        return sliderColliderDisableCount <= 0;
    }

    public void SetBorderCollider(int index, bool isActive)
    {
        borderColliders[index].SetActive(isActive);
    }

    public void SetBorderColliders(bool isActive)
    {
        foreach (GameObject g in borderColliders)
            g.SetActive(isActive);
    }

    public bool CanMove(int x, int y)
    {
        if (hasAnchor)
            return false;

        return true;
    }

    // CanRotate() => no anchor and not linked

    
    // Use this one usually!
    public void SetGridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        Vector3 newPos = STILE_WIDTH * new Vector3(x, y);

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

    public void SetGridPositionRaw(int x, int y)
    {
        this.x = x;
        this.y = y;
        Vector3 newPos = STILE_WIDTH * new Vector3(x, y);
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
    }

    public Vector2 GetMovingDirection()
    {
        return movingDirection;
    }

    public void SetMovingPosition(Vector2 position)
    {
        Vector3 newPos = STILE_WIDTH * new Vector3(position.x, position.y);

        // physics
        Vector3 dr = newPos - transform.position;
        UpdateTilePhysics(dr);


        transform.position = newPos;
        SetTileMapPositions(newPos);
    }

    private void UpdateTilePhysics(Vector3 dr)
    {
        // if player is on stile, move them
        //              THIS IS TEMPORARY, REPLACE WITH PROPPER CHECK ON ALL SLIDEABLES
        int playerIsland = Player.GetStileUnderneath();
        if (playerIsland == islandId)
        {
            Player.SetPosition(Player.GetPosition() + dr);
        }
    }


    private void SetTileMapPositions(Vector3 pos)
    {
        pos = pos + new Vector3(-0.5f, -0.5f);

        stileTileMaps.transform.position = pos;
    }
}
