using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[CustomEditor(typeof(WorldNavigation))]
public class WorldNavigationEditor : Editor
{
    private WorldNavigation _target;

    private void OnEnable()
    {
        _target = (WorldNavigation)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Set Path To Debug"))
        {
            _target.SetPathToDebug();
        }
    }
}
