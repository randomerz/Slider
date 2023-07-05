using SliderVocalization;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Interpolated / managed parameters during narration
/// </summary>
public class VocalizationContext
{
    public Transform root;
    public VocalizableParagraph topLevelParent;
    public VocalizationContext(Transform root, VocalizableParagraph topLevelParent)
    {
        this.root = root;
        this.topLevelParent = topLevelParent;
    }

    public float vowelOpenness;
    public float vowelForwardness;
}

/// <summary>
/// Non-interpolated parameters during narration.
/// </summary>
public class VocalRandomizationContext
{
    public float wordPitchBase;
    public float wordPitchIntonated;
    public float lastWordFinalPitch;
    public bool isCurrentWordLow;
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