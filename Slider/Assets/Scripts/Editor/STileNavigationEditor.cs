using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[CustomEditor(typeof(STileNavigation))]
public class STileNavigationEditor : Editor
{
    private STileNavigation _target;

    private void OnEnable()
    {
        _target = (STileNavigation)target;
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
