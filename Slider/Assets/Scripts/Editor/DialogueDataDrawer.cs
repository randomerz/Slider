using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueData))]
public class DialogueDataDrawer : PropertyDrawer
{
    private const string DIALOGUE_PROPERTY_NAME = "dialogue";
    private readonly string[] PROGRAMMY_PROPERTY_NAMES = {
        "delayAfterFinishedTyping",
        "waitUntilPlayerAction",
        "advanceDialogueManually",
        "doNotRepeatEvents",
        "dontInterrupt",
        "onDialogueStart",
        "onDialogueEnd",
    };

    // public static bool isProgrammyUnfolded = false;

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Foldout
        var foldoutRect = GetLinePositionFrom(position, 1);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
        position.y += 20;

        if (property.isExpanded)
        {
            // Dialogue
            var dialogueRect = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight * 3
            );
            SerializedProperty prop = property.FindPropertyRelative(DIALOGUE_PROPERTY_NAME);
            EditorGUI.PropertyField(dialogueRect, property.FindPropertyRelative(DIALOGUE_PROPERTY_NAME), true);
            position.y += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing;

            // Programmy Foldout -- were just storing it in the dialogue's `isExpanded` :)
            var programmyFoldoutRect = GetLinePositionFrom(position, 1);
            prop.isExpanded = EditorGUI.Foldout(programmyFoldoutRect, prop.isExpanded, "Programmy Parts", true);
            position.y += 20;
            
            if (prop.isExpanded)
            {
                foreach (string s in PROGRAMMY_PROPERTY_NAMES)
                {
                    prop = property.FindPropertyRelative(s);
                    EditorGUI.PropertyField(position, property.FindPropertyRelative(s), true);
                    position.y += EditorGUI.GetPropertyHeight(prop);
                }
            }
        }

        // EditorGUI.PropertyField(position, property, label, true);
    }
    
 
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0;

        // Base foldout
        height += 20;
        
        if (property.isExpanded)
        {
            // Dialogue
            SerializedProperty prop = property.FindPropertyRelative(DIALOGUE_PROPERTY_NAME);
            height += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing;

            // Programmy foldout
            height += 20;

            if (prop.isExpanded)
            {
                foreach (string s in PROGRAMMY_PROPERTY_NAMES)
                {
                    prop = property.FindPropertyRelative(s);
                    height += EditorGUI.GetPropertyHeight(prop);
                }
            }
        }

        // height += EditorGUI.GetPropertyHeight(property);

        return height;
    }

    private Rect GetLinePositionFrom(Rect rect, int line)
    {
        float heightModifier = EditorGUIUtility.singleLineHeight * (line - 1);

        return new Rect(
            rect.x,
            rect.y + heightModifier,
            rect.width,
            EditorGUIUtility.singleLineHeight);
    }
}