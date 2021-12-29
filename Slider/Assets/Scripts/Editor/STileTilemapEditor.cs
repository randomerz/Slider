using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[CustomEditor(typeof(STileTilemap))]
public class STileTilemapEditor : Editor
{
    private STileTilemap _target;

    private void OnEnable()
    {
        _target = (STileTilemap)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Set Walls as Colliders"))
        {
            _target.SetWallsAsColliders();
        }

        if (GUILayout.Button("Clear Colliders"))
        {
            _target.ClearColliders();
        }
    }
}
