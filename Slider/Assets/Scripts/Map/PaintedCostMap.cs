using UnityEngine;
using UnityEngine.Tilemaps;

public class PaintedCostMap : MonoBehaviour 
{
    [SerializeField] private PaintedSTileCostMap[] stileCostMaps;
    [SerializeField] private TileBase[] weightTiles;

    private int stileWidth;

    private void Awake() 
    {
        stileWidth = stileCostMaps[0].myStile.STILE_WIDTH;
    }

    public float GetNormalizedCostAt(Vector2 worldCoords)
    {
        STile stile = GetStileAt(worldCoords);
        if (stile == null)
            return 1;
        
        PaintedSTileCostMap stileCostMap = stileCostMaps[stile.islandId - 1];
        int stilePositionX = (int)(worldCoords.x - stile.transform.position.x - 0.5f);
        int stilePositionY = (int)(worldCoords.y - stile.transform.position.y + 0.5f);
        TileBase tileBase = stileCostMap.GetTileAt(stilePositionX, stilePositionY);

        int tileIndex = -1;
        for (int i = 0; i < weightTiles.Length; i++)
        {
            if (weightTiles[i] == tileBase)
            {
                tileIndex = i;
                break;
            }
        }

        if (tileIndex == -1)
        {
            //Tile is untraversable.
            return 2;
        }

        return tileIndex / (weightTiles.Length - 1.0f);
    }

    // Returns island id
    private STile GetStileAt(Vector2 worldCoords)
    {
        int stileX = (int)((worldCoords.x + (stileWidth / 2) + 0.5f) / stileWidth);
        int stileY = (int)((worldCoords.y + (stileWidth / 2) + 0.5f) / stileWidth);

        STile stile = SGrid.Current.GetStileAt(stileX, stileY);

        if (stile.isTileActive && !stile.IsMoving())
        {
            return stile;
        }

        // else look for the moving stile

        return null;
    }
}