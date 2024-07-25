using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RailManager : MonoBehaviour
{
    public Tilemap railMap;
    public List<RailTile> stateZeroTiles = new List<RailTile>(24);
    public List<RailTile> stateOneTiles = new List<RailTile>(24); //There are only 24 junctions, so there are only 24 tiles with alternate states
    public List<Vector3Int> railLocations = new List<Vector3Int>(); // a list of the locations of the rail tiles 
    public Minecart mc;
    public bool isBorderRM = false; //this is really jank but for now it works
    public RailManager otherRMOnTile;

    private void Start() 
    {
        foreach (Vector3Int position in railMap.cellBounds.allPositionsWithin) {
            TileBase tile = railMap.GetTile(position);
            if (tile != null) {
                railLocations.Add(position);
            }
        }
    }

    public void SetTile(Vector3Int loc, RailTile tile)
    {
        railMap.SetTile(loc, tile);
        railMap.RefreshTile(loc);
    }

    public void ChangeTile(Vector3Int loc) 
    {
        ChangeTile(loc, railMap.GetTile(loc) as RailTile);
    }

    public void ChangeTile(Vector3Int loc, RailTile tile)
    {
        int index = stateOneTiles.IndexOf(tile);
        if (index > -1)
            SetTile(loc, stateZeroTiles[index]);
    }

    public void ChangeAllTiles()
    {
        foreach (Vector3Int position in railMap.cellBounds.allPositionsWithin) {
            RailTile tile = railMap.GetTile(position) as RailTile;
            if (tile != null) {
                ChangeTile(position, tile);
            }
        }
    }
}
