using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NPCConditionals))]
public class NPCConditionalsDrawer : PropertyDrawer
{
    private readonly string[] PROPERTY_NAMES = {
        "conditionType",
        "conditions",
        "alwaysStartFromBeginning",
        "dialogueChain",
    };
    private readonly string[] ANIMATOR_PROPERTY_NAMES = {
        "animationOnEnter",
        // "animationOnExhaust",
        "emoteOnEnter",
        // "emoteOnExhaust",
    };
    private readonly string[] PROGRAMMER_PROPERTY_NAMES = {
        "onConditionalEnter",
        "onDialogueChainExhausted",
        "walks",
    };
    private const string IS_ANIMATOR_UNFOLDED_NAME = "editorIsAnimatorUnfolded";
    private const string IS_PROGRAMMER_UNFOLDED_NAME = "editorIsProgrammerUnfolded";

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
            
            position.y += 10; // Space after dialogue chain

            // Animator Foldout
            prop = property.FindPropertyRelative(IS_ANIMATOR_UNFOLDED_NAME);
            var animatorFoldoutRect = GetLinePositionFrom(position, 1);
            prop.boolValue = EditorGUI.Foldout(animatorFoldoutRect, prop.boolValue, "Animator Parts", true);
            position.y += 20;
            
            if (prop.boolValue)
            {
                foreach (string s in ANIMATOR_PROPERTY_NAMES)
                {
                    prop = property.FindPropertyRelative(s);
                    EditorGUI.PropertyField(position, property.FindPropertyRelative(s), true);
                    position.y += EditorGUI.GetPropertyHeight(prop);
                }
            }

            // Programmy Foldout
            prop = property.FindPropertyRelative(IS_PROGRAMMER_UNFOLDED_NAME);
            var programmyFoldoutRect = GetLinePositionFrom(position, 1);
            prop.boolValue = EditorGUI.Foldout(programmyFoldoutRect, prop.boolValue, "Programmer Parts", true);
            position.y += 20;
            
            if (prop.boolValue)
            {
                foreach (string s in PROGRAMMER_PROPERTY_NAMES)
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

            height += 10; // Space after dialogue chain

            // Animator foldout
            height += 20;

            prop = property.FindPropertyRelative(IS_ANIMATOR_UNFOLDED_NAME);
            if (prop.boolValue)
            {
                foreach (string s in ANIMATOR_PROPERTY_NAMES)
                {
                    prop = property.FindPropertyRelative(s);
                    height += EditorGUI.GetPropertyHeight(prop);
                }
            }

            // Programmy foldout
            height += 20;

            prop = property.FindPropertyRelative(IS_PROGRAMMER_UNFOLDED_NAME);
            if (prop.boolValue)
            {
                foreach (string s in PROGRAMMER_PROPERTY_NAMES)
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