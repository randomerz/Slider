using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slider : MonoBehaviour
{
    public bool isEmpty;

    public int xPos;
    public int yPos;

    private const int SLIDER_WIDTH = 17;

    [Header("References")]
    public GameObject floorTileGrid;
    public GameObject wallTileGrid;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetPosition(int x, int y)
    {
        xPos = x;
        yPos = y;
        Vector3 newPos = SLIDER_WIDTH * new Vector3(x, y);

        if (!isEmpty)
        {
            // animations and style
            floorTileGrid.transform.position = newPos;
            wallTileGrid.transform.position = newPos;
        }
        else
        {
            floorTileGrid.transform.position = newPos;
            wallTileGrid.transform.position = newPos;
        }

        transform.position = newPos;
    }
}
