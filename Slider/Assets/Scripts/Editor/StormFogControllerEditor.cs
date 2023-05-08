using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FogAnimationController))]
public class StormFogControllerEditor : Editor
{
    private FogAnimationController _target;

    private void OnEnable()
    {
        _target = (FogAnimationController)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Randomize Fog Positions"))
        {
            _target.RandomizeFogPositions();
        }
    }
}
