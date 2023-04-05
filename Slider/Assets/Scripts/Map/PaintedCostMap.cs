using UnityEngine;
using UnityEngine.Tilemaps;

public class PaintedCostMap : MonoBehaviour 
{
    [SerializeField] private PaintedSTileCostMap[] stileCostMaps;
    [SerializeField] private TileBase[] weightTiles;
    [SerializeField] private STile[] stiles;

    private int stileWidth;

    private void Awake() 
    {
        stileWidth = stileCostMaps[0].myStile.STILE_WIDTH;
    }

    public float GetNormalizedCostAt(Vector2 worldCoords)
    {
        STile stile = GetStileAt(worldCoords);
        if (stile == null)
        {
            return 2;   //Tile untraversable.
        }
        
        PaintedSTileCostMap stileCostMap = stileCostMaps[stile.islandId - 1];
        int stilePositionX = (int)(worldCoords.x - stile.transform.position.x);
        int stilePositionY = (int)(worldCoords.y - stile.transform.position.y);
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

    private STile GetStileAt(Vector2 worldCoords)
    {
        float halfWidth = (float) stileWidth / 2;
        foreach (STile st in stiles)
        {
            if (!st.isTileActive)
                continue;
                
            float dx = worldCoords.x - st.transform.position.x;
            float dy = worldCoords.y - st.transform.position.y;
            if (dx >= -halfWidth && dx <= halfWidth && dy >= -halfWidth && dy <= halfWidth) {
                return st;
            }
        }

        //Debug.LogError("Couldn't Find Stile for Painted Cost Map (this shouldn't happen?)");
        return null;

        //int stileX = (int)((worldCoords.x + halfWidth + 0.5f) / stileWidth);
        //int stileY = (int)((worldCoords.y + halfWidth + 0.5f) / stileWidth);

        //STile stile = SGrid.Current.GetStileAt(stileX, stileY);

        //if (stile.isTileActive && !stile.IsMoving())
        //{
        //    return stile;
        //}

        //// else look for the moving stile
        //return null;
    }
}