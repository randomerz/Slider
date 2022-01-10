using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class STileTilemap : MonoBehaviour
{
    public Tilemap floor;
    public Tilemap walls;
    public Tilemap decoration;
    public Tilemap colliders;


    public void ClearColliders()
    {
        colliders.ClearAllTiles();
    }

    public void SetWallsAsColliders()
    {
        Vector3 size = walls.size;
        for (int x = walls.cellBounds.min.x; x < walls.cellBounds.max.x; x++)
        {
            for (int y = walls.cellBounds.min.x; y < walls.cellBounds.max.x; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                colliders.SetTile(pos, walls.GetTile(pos));
            }
        }
    }
}
