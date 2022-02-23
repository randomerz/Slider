using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[CustomEditor(typeof(MomentumGizmo))]
public class MomentumGizmoEditor : Editor
{
    private MomentumGizmo _target;
    private bool autoSet = true;

    private void OnEnable()
    {
        _target = (MomentumGizmo)target;

        if (autoSet)
        {
            // FindSTile();
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // autoSet = GUILayout.Toggle(autoSet, "Auto-Set mySTile");
        // if (autoSet && GUI.changed)
        // {
        //     // FindSTile();
        // }

        // if (GUILayout.Button("Clear Colliders"))
        // {
        //     // FindSTile();
        // }
    }

    private void FindSTile()
    {
        Transform curr = _target.transform;
        int i = 0;
        while (curr.parent != null && i < 100)
        {
            if (curr.GetComponent<STile>() != null)
            {
                _target.myStile = curr.GetComponent<STile>();
                return;
            }

            // Debug.Log(curr.name);
            curr = curr.parent;
            i += 1;
        }

        if (i == 100)
            Debug.LogWarning("something went wrong in custom editor!");
    }
}
