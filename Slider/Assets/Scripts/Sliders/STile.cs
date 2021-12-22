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
    
    private const int STILE_WIDTH = 17;
    
    [Header("References")]
    public Collider2D sliderCollider;
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

    public bool CanMove(int x, int y)
    {
        if (hasAnchor)
            return false;

        if (linkTile != null)
            return false;

        return true;
    }

    // CanRotate() => no anchor and not linked

        
    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        Vector3 newPos = STILE_WIDTH * new Vector3(x, y);
        //Debug.Log("new position of tile " + islandId + ": " + newPos);

        //StartCoroutine(StartCameraShakeEffect());

        if (isTileActive)
        {
            // animations and style

            //StartCoroutine(StartMovingAnimation(transform.position, newPos, Player.GetSliderUnderneath() == islandId, Player.GetPosition() - transform.position));

            // temp
            transform.position = newPos;
            SetTileMapPositions(newPos);
        }
        else
        {
            transform.position = newPos;
            SetTileMapPositions(newPos);
        }
    }

    public void SetPositionRaw(int x, int y)
    {
        this.x = x;
        this.y = y;
        Vector3 newPos = STILE_WIDTH * new Vector3(x, y);
        transform.position = newPos;
        SetTileMapPositions(newPos);
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
