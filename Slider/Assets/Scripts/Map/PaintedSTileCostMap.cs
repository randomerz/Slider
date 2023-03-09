using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class PaintedSTileCostMap : MonoBehaviour 
{
    public STile myStile;
    [SerializeField] private Tilemap weightsTilemap;
    
    public TileBase GetTileAt(int x, int y)
    {
        return weightsTilemap.GetTile(new Vector3Int(x, y, 0));
    }
}