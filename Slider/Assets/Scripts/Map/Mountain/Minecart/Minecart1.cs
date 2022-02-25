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
    [SerializeField] public RailTile currentTile;
    [SerializeField] public RailTile targetTile;
    [SerializeField] private float speed = 2.0f;
    public Vector3 offSet = new Vector3(0.5f, 0.5f, 0.0f);


    public Vector3Int currentTilePos;
    public Vector3Int targetTilePos; 
    public Vector3 targetWorldPos;
    Coroutine move;


    //creates a new minecart at the given position
    public Minecart(RailManager rm, Vector3Int pos){
      railManager = rm;
      SnapToTile(pos);
    }


    //Item Related Stuff
    public override STile DropItem(Vector3 dropLocation, System.Action callback=null) 
    {
        Collider2D hit = Physics2D.OverlapPoint(dropLocation, LayerMask.GetMask("Slider"));
        if (hit == null || hit.GetComponent<STile>() == null)
        {
            gameObject.transform.parent = null;
            //Debug.LogWarning("Player isn't on top of a slider!");
            return null;
        }
        STile hitTile = hit.GetComponent<STile>();
        Tilemap railmap = hitTile.stileTileMaps.GetComponent<STileTilemap>().minecartRails;
        railManager = railmap.GetComponent<RailManager>();
        StartCoroutine(AnimateDrop(railmap.CellToWorld(railmap.WorldToCell(dropLocation)) + mc.offSet, callback));
        SnapToTile(railmap.WorldToCell(dropLocation));
        gameObject.transform.parent = hitTile.transform.Find("Objects").transform;
        return hitTile;
    }

    public override void OnEquip()
    {
        StopMoving();
        resetTiles();
        base.OnEquip();
    }




    public void StartMoving() 
    {
      if(isOnTrack)
        isMoving = true;  
    }

    public void StopMoving()
    {
      isMoving = false;
      isOnTrack = false;
    }

    public void resetTiles()
    {
      currentTile = null;
      targetTile = null;
      currentTilePos = Vector3Int.zero;
      targetTilePos = Vector3Int.zero;
    }

    //Places the minecart on the tile at the given position
    public void SnapToTile(Vector3Int pos)
    {
      transform.position = railManager.railMap.layoutGrid.CellToWorld(pos) + offSet;
      currentTile = railManager.railMap.GetTile(pos) as RailTile;
      currentTilePos = pos;
      currentDirection = currentTile.defaultDir;
      if(railManager.railLocations.Contains(pos))
      {
          targetTilePos = currentTilePos + getTileOffsetVector(currentDirection);
          targetTile = railManager.railMap.GetTile(targetTilePos) as RailTile;
          targetWorldPos = railManager.railMap.layoutGrid.CellToWorld(targetTilePos) + 0.5f * (Vector3) getTileOffsetVector(targetTile.connections[(currentDirection + 2) % 4]) + offSet;
          isOnTrack = true;
      }
      else
      {
          resetTiles();
      }
      
    }

    private void Update() 
    {
      if(isMoving && isOnTrack)
      {
        //Debug.Log(Vector3.Distance(transform.position, targetWorldPos));
        if(Vector3.Distance(transform.position, targetWorldPos) < 0.01f)
        {
          currentTile = targetTile;
          currentTilePos = targetTilePos;
          targetTilePos = currentTilePos + getTileOffsetVector(currentDirection);
          targetTile = railManager.railMap.GetTile(targetTilePos) as RailTile;
          targetWorldPos = railManager.railMap.layoutGrid.CellToWorld(targetTilePos) 
                          + 0.5f * (Vector3) getTileOffsetVector(targetTile.connections[(currentDirection + 2) % 4]) + offSet;
          currentDirection = targetTile.connections[(currentDirection + 2) % 4];
        }
        else
        {
          transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, Time.deltaTime * speed);
        }
      }
    }

    //returns a vector that can be added to the tile position in order to determine the location of the specified point
    public static Vector3Int getTileOffsetVector(int num)
    {
      int[] arr = {1, 0, -1, 0};
      return new Vector3Int(arr[num], arr[(num+3) % 4], 0);
    }
}
