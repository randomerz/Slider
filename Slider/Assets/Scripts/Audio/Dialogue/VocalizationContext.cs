using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Collection of parameters guiding a current vocalization instance. Shared between different levels of vocalization
/// </summary>
public class VocalizationContext
{
    public Transform root;
    public VocalizationContext(Transform root)
    {
        this.root = root;
    }

    #region SENTENCE VOCALIZER RESPONSIBILITIES
    public float wordPitchBase;
    public float wordPitchIntonated;
    public bool isCurrentWordLow;
    #endregion

    #region WORD VOCALIZER RESPONSIBILITIES
    #endregion

    #region PHONEME CLUSTER VOCALIZER RESPONSIBILITIES
    public float vowelOpeness;
    public float vowelForwardness;
    #endregion
}

#if UNITY_EDITOR
// unity readonly drawer
[CustomPropertyDrawer(typeof(VocalizationContext))]
public class VocalizationContextDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif