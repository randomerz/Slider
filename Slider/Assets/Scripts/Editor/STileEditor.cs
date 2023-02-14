using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[CustomEditor(typeof(STile))]
public class STileEditor : Editor
{
    private STile _target;
    private bool autoUpdatePostition = true;

    private void OnEnable()
    {
        _target = (STile)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        autoUpdatePostition = EditorGUILayout.Toggle("Auto-Update position", autoUpdatePostition);
        if (autoUpdatePostition)
        {
            _target.transform.position = new Vector3(_target.x, _target.y) * _target.STILE_WIDTH;
        }
    }
}
