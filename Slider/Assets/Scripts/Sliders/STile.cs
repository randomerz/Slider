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
    public STile linkTile;

    private Vector2 movingDirection; // zero, right, up, left, down
    
    private const int STILE_WIDTH = 17;
    
    [Header("References")]
    public Collider2D sliderCollider;
    public GameObject tileMapCollider;
    public GameObject[] borderColliders; // right top left bottom
    public GameObject stileTileMaps;
    //public GameObject floorTileGrid;
    //public GameObject wallTileGrid;
    //public GameObject decorationsTileGrid;
    //public GameObject collidersTileGrid;

    private void Awake()
    {
        SetTileActive(isTileActive);
        Vector3 defaultPos = STILE_WIDTH * new Vector3(x, y);
        transform.position = defaultPos;
        SetTileMapPositions(defaultPos);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetTileActive(bool isTileActive)
    {
        this.isTileActive = isTileActive;

        stileTileMaps.SetActive(isTileActive);
        //floorTileGrid.SetActive(isTileActive);
        //wallTileGrid.SetActive(isTileActive);
        //decorationsTileGrid.SetActive(isTileActive);
        //collidersTileGrid.SetActive(isTileActive);

        sliderCollider.isTrigger = isTileActive;
    }

    public void SetSliderCollider(bool isActive)
    {
        sliderCollider.enabled = isActive;
        tileMapCollider.SetActive(isActive);
    }

    public void SetBorderCollider(bool isActive)
    {
        foreach (GameObject g in borderColliders)
        {
            g.SetActive(isActive);
        }
    }

    public bool CanMove(int x, int y)
    {
        if (hasAnchor)
            return false;

        if (linkTile != null)
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
        //floorTileGrid.transform.position = pos;
        //wallTileGrid.transform.position = pos;
        //decorationsTileGrid.transform.position = pos;
        //collidersTileGrid.transform.position = pos;
    }
}
