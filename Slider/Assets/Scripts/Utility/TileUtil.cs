using UnityEngine;

public class TileUtil
{

    public static Vector2Int WorldToTileCoords(Vector2 posInWorld)
    {
        //L: Tiles are positioned based on the bottom left corner, so need to add small amount to avoid off by 1 error.
        return new Vector2Int(Mathf.RoundToInt(posInWorld.x + 0.01f), Mathf.RoundToInt(posInWorld.y + 0.01f));
    }
}