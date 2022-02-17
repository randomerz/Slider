using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minecart : MonoBehaviour
{
    [SerializeField] private int currentDirection;
    [SerializeField] private bool isOnTrack;
    [SerializeField] private bool isMoving;
    [SerializeField] private RailTile currentTile;
    [SerializeField] private RailTile nextTile;
    [SerializeField] private int speed;

    //Places the minecart on the given tile
    public void SnapToTile(RailTile tile)
    {
      //  transform.position = GetTileData();
    }

    
}
