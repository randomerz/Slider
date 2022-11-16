using UnityEngine;

public class TileUtil
{
    private const float epsilon = 0.001f;

    public static Vector2Int WorldToTileCoords(Vector2 posInWorld)
    {
        //L: Tiles are positioned based on the bottom left corner, so need to add small amount to avoid off by 1 error.
        return new Vector2Int(Mathf.RoundToInt(posInWorld.x+epsilon), Mathf.RoundToInt(posInWorld.y+epsilon));
    }
}