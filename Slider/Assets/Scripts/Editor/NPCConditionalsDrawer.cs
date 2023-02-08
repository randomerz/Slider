using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NPCConditionals))]
public class NPCConditionalsDrawer : PropertyDrawer
{
    private readonly string[] PROPERTY_NAMES = {
        "conditions",
        "alwaysStartFromBeginning",
        "dialogueChain",
    };
    private readonly string[] PROGRAMMY_PROPERTY_NAMES = {
        "onConditionalEnter",
        "onDialogueChainExhausted",
        "walks",
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
            SerializedProperty prop;
            
            // Normal properties
            foreach (string s in PROPERTY_NAMES)
            {
                prop = property.FindPropertyRelative(s);
                EditorGUI.PropertyField(position, property.FindPropertyRelative(s), true);
                position.y += EditorGUI.GetPropertyHeight(prop);
            }

            // Programmy Foldout -- were just storing it in the `onConditionalEnter` :)
            prop = property.FindPropertyRelative("onConditionalEnter");
            var programmyFoldoutRect = GetLinePositionFrom(position, 1);
            prop.isExpanded = EditorGUI.Foldout(programmyFoldoutRect, prop.isExpanded, "Other Programmy Parts", true);
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
            SerializedProperty prop;
            
            // Normal properties
            foreach (string s in PROPERTY_NAMES)
            {
                prop = property.FindPropertyRelative(s);
                height += EditorGUI.GetPropertyHeight(prop);
            }

            // Programmy foldout
            height += 20;

            prop = property.FindPropertyRelative("onConditionalEnter");
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