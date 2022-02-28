using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightManager))]
public class LightManagerEditor : Editor
{
    private LightManager _target;

    private void OnEnable()
    {
        _target = (LightManager) target;
    }
    public override void OnInspectorGUI()
    {
        //Called whenever the inspector is drawn for this object.
        DrawDefaultInspector();
        //This draws the default screen.  You don't need this if you want
        //to start from scratch, but I use this when I'm just adding a button or
        //some small addition and don't feel like recreating the whole inspector.
        if (GUILayout.Button("Do Lighting Pre-Calculations"))
        {
            //add everthing the button would do.

            _target.DoLightingPreCalculations();
        }
    }
}