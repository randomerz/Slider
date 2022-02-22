using UnityEngine;

public class TileUtil
{
    //L: Get the tile coordinates of a position (useful for checking if the player is standing on a specific tile)
    public static Vector3Int WorldToTileCoords(Vector3 posInWorld)
    {
        //L: Tiles are positioned based on the bottom left corner, so need to add (0.5f, 0.5f) to avoid off by 1 error.
        return new Vector3Int(Mathf.RoundToInt(posInWorld.x + 0.5f), Mathf.RoundToInt(posInWorld.y + 0.5f), 0);
    }
}

