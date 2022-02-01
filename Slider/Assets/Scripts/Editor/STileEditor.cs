using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[CustomEditor(typeof(STile))]
public class STileEditor : Editor
{
    private STile _target;

    private void OnEnable()
    {
        _target = (STile)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Toggle(true, "Auto-Update position") && GUI.changed)
        {
            _target.transform.position = new Vector3(_target.x, _target.y) * _target.STILE_WIDTH;
        }
    }
}
