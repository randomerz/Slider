using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// IngredientDrawer
[CustomPropertyDrawer(typeof(DialogueData))]
public class DialogueDataDrawer : PropertyDrawer
{
    private const string DIALOGUE_PROPERTY_NAME = "dialogue";
    private readonly string[] property_names = {
        "delayAfterFinishedTyping",
        "waitUntilPlayerAction",
        "advanceDialogueManually",
        "doNotRepeatEvents",
        "dontInterrupt",
        "onDialogueStart",
        "onDialogueEnd",
    };

    bool isFolded = true;

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

            foreach (string s in property_names)
            {
                prop = property.FindPropertyRelative(s);
                EditorGUI.PropertyField(position, property.FindPropertyRelative(s), true);
                position.y += EditorGUI.GetPropertyHeight(prop);
            }
        }

        // EditorGUI.PropertyField(position, property, label, true);
    }
    
 
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0;

        height += 20;
        
        if (property.isExpanded)
        {
            SerializedProperty prop = property.FindPropertyRelative(DIALOGUE_PROPERTY_NAME);
            height += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing;

            foreach (string s in property_names)
            {
                prop = property.FindPropertyRelative(s);
                height += EditorGUI.GetPropertyHeight(prop);
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