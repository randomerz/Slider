using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

//L: See CaveMossManager.cs for how these tiles are handled.
public class MossTile : Tile
{
    //L: Sprite Swap (not needed since using multiple layers)
    //public MossTile linkedTile;

#if UNITY_EDITOR
    [MenuItem("Assets/Create/2D/Tiles/Custom Tiles/Moss Tile")]
    public static void CreateMossTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Moss Tile", "New Moss Tile", "Asset", "Save Moss Tile", AssetDatabase.GetAssetPath(Selection.activeObject));
        if (path == "") return;

        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MossTile>(), path);
    }
#endif
}
