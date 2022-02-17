using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RailTile : Tile
{
    public int[] connections = {-1, -1, -1, -1};




/*
    private void Start() {
    }
    public override void RefreshTile(Vector3Int location, ITilemap tilemap)
    {
        for (int yd = -1; yd <= 1; yd++)
            for (int xd = -1; xd <= 1; xd++)
            {
                Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                if (HasRailTile(tilemap, position))
                    tilemap.RefreshTile(position);
            }
    }
    
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprites[state];
    }

    public void UpdateState(int num)
    {
        state = 1;
        //GetTileData(this.);
    }



    private bool HasRailTile(ITilemap tilemap, Vector3Int position)
    {
        return tilemap.GetTile(position) == this;
    }
*/
    #if UNITY_EDITOR
    [MenuItem("Assets/Create/2D/Tiles/Custom Tiles/Rail Tile")]
    public static void CreateRailTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Rail Tile", "New Rail Tile", "Asset", "Save Rail Tile", AssetDatabase.GetAssetPath(Selection.activeObject));
        if (path == "") return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RailTile>(), path);
    }
    #endif
}
