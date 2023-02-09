using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueData))]
public class DialogueDataDrawer : PropertyDrawer
{
    private const string DIALOGUE_PROPERTY_NAME = "dialogue";
    private readonly string[] ANIMATOR_PROPERTY_NAMES = {
        "animationOnStart",
        "animationOnLeave",
        "emoteOnStart",
        "emoteOnLeave",
    };
    // private const string ANIMATOR_ON_START_NAME = "animationOnStart";
    // private const string ANIMATOR_ON_LEAVE_NAME = "animationOnLeave";
    // private string[] availableStates;

    private readonly string[] PROGRAMMER_PROPERTY_NAMES = {
        "delayAfterFinishedTyping",
        "waitUntilPlayerAction",
        "advanceDialogueManually",
        "doNotRepeatEvents",
        "dontInterrupt",
        "onDialogueStart",
        "onDialogueEnd",
    };
    private const string IS_ANIMATOR_UNFOLDED_NAME = "editorIsAnimatorUnfolded";
    private const string IS_PROGRAMMER_UNFOLDED_NAME = "editorIsProgrammerUnfolded";

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

            // Animator Foldout
            prop = property.FindPropertyRelative(IS_ANIMATOR_UNFOLDED_NAME);
            var animatorFoldoutRect = GetLinePositionFrom(position, 1);
            prop.boolValue = EditorGUI.Foldout(animatorFoldoutRect, prop.boolValue, "Animator Parts", true);
            position.y += 20;
            
            if (prop.boolValue)
            {
                EditorGUI.indentLevel++;
                
                // animationOnStart
                // couldnt get animations to work :(
                // check thread https://stackoverflow.com/questions/62795915/unity-custom-drop-down-selection-for-arrays-c-sharp
                // prop = property.FindPropertyRelative("npcAnimatorController");
                // Debug.Log("prop:" + prop);

                foreach (string s in ANIMATOR_PROPERTY_NAMES)
                {
                    prop = property.FindPropertyRelative(s);
                    EditorGUI.PropertyField(position, property.FindPropertyRelative(s), true);
                    position.y += EditorGUI.GetPropertyHeight(prop);
                }

                EditorGUI.indentLevel--;
            }

            // Programmy Foldout
            prop = property.FindPropertyRelative(IS_PROGRAMMER_UNFOLDED_NAME);
            var programmyFoldoutRect = GetLinePositionFrom(position, 1);
            prop.boolValue = EditorGUI.Foldout(programmyFoldoutRect, prop.boolValue, "Programmer Parts", true);
            position.y += 20;
            
            if (prop.boolValue)
            {
                EditorGUI.indentLevel++;
                
                foreach (string s in PROGRAMMER_PROPERTY_NAMES)
                {
                    prop = property.FindPropertyRelative(s);
                    EditorGUI.PropertyField(position, property.FindPropertyRelative(s), true);
                    position.y += EditorGUI.GetPropertyHeight(prop);
                }
                
                EditorGUI.indentLevel--;
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