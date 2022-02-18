using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RailManager : MonoBehaviour
{
    public Tilemap railMap;
    //public BoundsInt railBounds;
    GridInformation gridInfo;
    public List<RailTile> stateZeroTiles = new List<RailTile>();
    public List<RailTile> stateOneTiles = new List<RailTile>(); //There are only 24 junctions, so there are only 24 tiles with alternate states
    //public Tile[][] allTiles;
    public List<RailTile> occupiedTiles = new List<RailTile>(); // a list of the currently occupied rail tiles. Used to prevent switching when the minecart is on the junction 
    public Minecart mc;
  
    private void Start() 
    {
        //allTiles = new Tile[2][] {stateZeroTiles, stateOneTiles};
        foreach (Vector3Int position in railMap.cellBounds.allPositionsWithin) {
            TileBase tile = railMap.GetTile(position);
            if (tile != null) {
                Debug.Log(position);
                //HandleReplaceTile(tilemap, tile, position);
            }
        }

    }

    public void startMC()
    {
        mc.SnapToTile(new Vector3Int(1,1,0));
    }

    public void SetTile(Vector3Int loc, RailTile tile)
    {
        railMap.SetTile(loc, tile);
        railMap.RefreshTile(loc);
    }


    public void ChangeTile(Vector3Int loc, RailTile tile)
    {
        int index = stateOneTiles.IndexOf(tile);
        Debug.Log(tile);
        if (index > -1)
            SetTile(loc, stateZeroTiles[index]);
    }

    public void ChangeAllTiles()
    {
        foreach (Vector3Int position in railMap.cellBounds.allPositionsWithin) {
            RailTile tile = railMap.GetTile(position) as RailTile;
            if (tile != null) {
                Debug.Log(tile as RailTile);
                ChangeTile(position, tile as RailTile);
                //HandleReplaceTile(tilemap, tile, position);
            }
        }
    }

    /*public void printLocation() 
    {
        for(int x = railBounds.x; x < railBounds.xMax; x++)
        {
            for(int y = railBounds.y; y < railBounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x,y,railBounds.z);
                Tile tile = railMap.GetTile(pos) as Tile;
                if (tile != null)
                {
                    Debug.Log(pos);
                    Debug.Log(railMap.layoutGrid.CellToWorld(pos));
                }
            }
        }    
    }*/

    private void FindReplaceableTilesInTilemap(Tilemap tilemap) {
        foreach (var position in tilemap.cellBounds.allPositionsWithin) {
            TileBase tile = tilemap.GetTile(position);
            if (tile != null) {
                //HandleReplaceTile(tilemap, tile, position);
            }
        }
    }
 /*
    private void HandleReplaceTile(Tilemap tilemap, TileBase tile, Vector3Int position) {
        for (int i = 0, len = findTiles.Count; i < len; i++) {
            if (findTiles[i] == tile) {
                tilemap.SetTile(position, replaceTiles[i]);
            }
        }
    }*/




    internal void dummy()
    {
        //railMap.get
    }
}
/*
    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RailTile : Tile
{
    //public Sprite sprite;
    [SerializeField] private Sprite[] sprites;
[SerializeField] private int state {get; set;}

public RailTileData data;

/*Represents connections between points. ENWS = 0123. -1 is used for an unreachable direction. The first index represents the state
  IE [[1,3,-1,1][1,0,-1,1]] means that E->N, N->S, and S->N in state 0 and E->N, N->E, and S->N in stage 1   

[SerializeField] private int[,] indexMap = new int[2,4];
    
[SerializeField] private RailTile[] adjTiles;

 [SerializeField] private bool minecartOn; //is the minecart on this rail? (prevent switching junctions while minecart is using the junction)

 #if UNITY_EDITOR
[MenuItem("Assets/Create/2D/Tiles/Custom Tiles/Rail Tile")]
public static void CreateRailTile()
{
string path = EditorUtility.SaveFilePanelInProject("Save Rail Tile", "New Rail Tile", "Asset", "Save Rail Tile", AssetDatabase.GetAssetPath(Selection.activeObject));
if (path == "") return;
AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RailTile>(), path);
}
#endif



//returns the next railTile given the current direction of the minecart
  public RailTile getNextTile(int dir)
{
return adjTiles[indexMap[state,(dir + 2) % 4]];
}

    private void Update() {
    sprite = sprites[state];
  }

}*/

    
