using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RailTile : Tile
{
    //Direction corresponance: 0 = East, 1 = North, 2 = West, 3 = South

    //If the minecart enters in the direction of the index, this array contains the direction the minecart will leave the rail in
    //If the direction is inaccesable, -1 is used
    //for example, the EW straight has a connection array [2, -1, 0, -1] because entering from the east (0) leads to an exit to the west (2),
    //entering to the west(2) leads to an exit to the east (0), and you cannot enter the rail from the north or south
    public int[] connections = {-1, -1, -1, -1}; 
    
    public int defaultDir = 0; //the direction that the minecart will begin traveling in by default

    public bool isPowered = false;

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


