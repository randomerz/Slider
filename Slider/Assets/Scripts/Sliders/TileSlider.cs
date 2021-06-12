using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSlider : MonoBehaviour
{
    public bool isEmpty;

    public int islandId = -1;
    public int xPos;
    public int yPos;

    private const int SLIDER_WIDTH = 17;

    [Header("References")]
    public GameObject floorTileGrid;
    public GameObject wallTileGrid;

    void Awake()
    {
        SetEmpty(isEmpty);
        Vector3 defaultPos = SLIDER_WIDTH * new Vector3(xPos, yPos);
        transform.position = defaultPos;
        floorTileGrid.transform.position = defaultPos;
        wallTileGrid.transform.position = defaultPos;
    }

    void Update()
    {
        
    }

    public void SetPosition(int x, int y)
    {
        xPos = x;
        yPos = y;
        Vector3 newPos = SLIDER_WIDTH * new Vector3(x, y);
        //Debug.Log("new position of tile " + islandId + ": " + newPos);

        if (!isEmpty)
        {
            // animations and style
            if (Player.GetSliderUnderneath() == islandId)
            {
                // set relative pos;
                Player.SetPosition(Player.GetPosition() - transform.position + newPos);
            }
            transform.position = newPos;
            floorTileGrid.transform.position = newPos;
            wallTileGrid.transform.position = newPos;
        }
        else
        {
            transform.position = newPos;
            floorTileGrid.transform.position = newPos;
            wallTileGrid.transform.position = newPos;
        }

        transform.position = newPos;
    }

    public void SetEmpty(bool isEmpty)
    {
        this.isEmpty = isEmpty;
        floorTileGrid.SetActive(!isEmpty);
        wallTileGrid.SetActive(!isEmpty);
    }
}
