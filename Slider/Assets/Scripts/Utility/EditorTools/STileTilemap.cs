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
    public Tilemap materials;
    public Tilemap minecartRails;

    public Tile wallTile;

    public void ClearColliders()
    {
        colliders.ClearAllTiles();
    }

    public void SetWallsAsColliders()
    {
        Vector3 size = walls.size;
        for (int x = walls.cellBounds.min.x; x < walls.cellBounds.max.x; x++)
        {
            for (int y = walls.cellBounds.min.y; y < walls.cellBounds.max.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (walls.GetTile(pos) != null)
                {
                    if (wallTile != null)
                        colliders.SetTile(pos, wallTile);
                    else
                        colliders.SetTile(pos, walls.GetTile(pos));
                }
            }
        }
    }
}
