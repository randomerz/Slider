using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Minecart : MonoBehaviour
{
    
    [SerializeField] private int currentDirection;
    public RailManager railManager;
    [SerializeField] private bool isOnTrack;
    [SerializeField] private bool isMoving;
    [SerializeField] public RailTile currentTile;
    [SerializeField] public RailTile targetTile;
    [SerializeField] private float speed = 2;
    public Vector3 offSet = new Vector3(0.5f, 0.5f, 0.0f);


    public Vector3Int currentTilePos;
    public Vector3Int targetTilePos; 
    //public Vector3 currentWorldPos;
    public Vector3 targetWorldPos;
    Coroutine move;


    //creates a new minecart at the given position
    public Minecart(RailManager rm, Vector3Int pos){
      railManager = rm;
      SnapToTile(pos);
    }

    //Places the minecart on the tile at the given position (currently hardcoded to get the next tile to the east)
    public void SnapToTile(Vector3Int pos)
    {
      transform.position = railManager.railMap.layoutGrid.CellToWorld(pos) + offSet;
      currentTile = railManager.railMap.GetTile(pos) as RailTile;
      currentTilePos = pos;
      currentDirection = 0;
      targetTilePos = currentTilePos + getTileOffsetVector(currentDirection);
      targetTile = railManager.railMap.GetTile(targetTilePos) as RailTile;
      targetWorldPos = railManager.railMap.layoutGrid.CellToWorld(targetTilePos) + 0.5f * (Vector3) getTileOffsetVector(targetTile.connections[(currentDirection + 2) % 4]) + offSet;
      isMoving = true;
      isOnTrack = true;
      //move = StartCoroutine(MoveMinecartCoroutine(transform.position, targetWorldPos));
    }

    private void Update() 
    {
      if(isMoving && isOnTrack)
      {
       // Debug.Log(Vector3.Distance(transform.position, targetWorldPos));
        if(Vector3.Distance(transform.position, targetWorldPos) < 0.01f)
        {
          currentTile = targetTile;
          currentTilePos = targetTilePos;
          targetTilePos = currentTilePos + getTileOffsetVector(currentDirection);
          targetTile = railManager.railMap.GetTile(targetTilePos) as RailTile;
          targetWorldPos = railManager.railMap.layoutGrid.CellToWorld(targetTilePos) + 0.5f * (Vector3) getTileOffsetVector(targetTile.connections[(currentDirection + 2) % 4]) + offSet;
          currentDirection = targetTile.connections[(currentDirection + 2) % 4];
          //move = StartCoroutine(MoveMinecartCoroutine(transform.position, targetWorldPos));
        }
        else
        {
          //Debug.Log(transform.position);
          transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, Time.deltaTime * speed);
          //transform.Translate((targetWorldPos - transform.position) * Time.deltaTime * speed);
        }
      }
    }

    //returns a vector that can be added to the tile position in order to determine the location of the specified point
    public static Vector3Int getTileOffsetVector(int num)
    {
      int[] arr = {1, 0, -1, 0};
      return new Vector3Int(arr[num], arr[(num+3) % 4], 0);
    }

    IEnumerator MoveMinecartCoroutine(Vector3 start, Vector3 target)
        {
            float time = 0f;
            while (transform.position != target)
            {
                transform.position = (Vector3.Lerp(start, target, (time / Vector3.Distance(start, target)) * speed));
                time += Time.deltaTime;
                yield return null;
            }
        }

    
}
