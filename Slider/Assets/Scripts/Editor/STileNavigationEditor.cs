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

        /* This needs to be done at runtime rn since it involves raycasts.
        if (GUILayout.Button("Bake Nav Grid"))
        {
            _target.BakeNavGrid();
        }
        */
    }
}
