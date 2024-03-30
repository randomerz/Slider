using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


[CustomPropertyDrawer(typeof(Condition))]
public class ConditionalsEditor : PropertyDrawer
{
    /*
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement();
        var categoryToDisplay = (Condition.ConditionType) property.FindPropertyRelative("type").enumValueIndex;
        var itemField = new PropertyField(property.FindPropertyRelative("item"));
        var patternField = new PropertyField(property.FindPropertyRelative("pattern"));
        var checkBoolField = new PropertyField(property.FindPropertyRelative("checkBool"));
        container.Add(new PropertyField(property.FindPropertyRelative("type")));
        // Check the value of the enum and display variables based on it
        switch (categoryToDisplay)
        {
            case Condition.ConditionType.item:
                container.Add(itemField);
                break;
            case Condition.ConditionType.grid:
                container.Add(patternField);
                break;
            case Condition.ConditionType.spec:
                container.Add(checkBoolField);
                break;
        }
        return container;
    }
    */

    bool showFields = true;


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 8;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var categoryProperty = property.FindPropertyRelative("type");
        var itemProperty = property.FindPropertyRelative("item");
        var patternProperty = property.FindPropertyRelative("pattern");
        var stationaryTilesProperty = property.FindPropertyRelative("stationaryTiles");
        var checkBoolProperty = property.FindPropertyRelative("checkBool");
        var playerItemNameProperty = property.FindPropertyRelative("playerItemName");
        var flagNameProperty = property.FindPropertyRelative("flagName");

        var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        var categoryFieldRect = new Rect(position.x, position.y + 18, position.width, 24);
        var fieldRect = new Rect(position.x, position.y + 2 * 18, position.width, 24);
        var field2Rect = new Rect(position.x, position.y + 3 * 18, position.width, 24);

        //EditorGUI.indentLevel++;
        showFields = EditorGUI.BeginFoldoutHeaderGroup(labelRect, showFields, label);
        EditorGUI.EndFoldoutHeaderGroup();

        if (showFields)
        {
            EditorGUI.PropertyField(categoryFieldRect, categoryProperty);

            switch ((Condition.ConditionType)categoryProperty.enumValueIndex)
            {
                case Condition.ConditionType.item:
                    EditorGUI.PropertyField(fieldRect, itemProperty, true);
                    break;
                case Condition.ConditionType.grid:
                case Condition.ConditionType.noGrid:
                    EditorGUI.PropertyField(fieldRect, patternProperty);
                    EditorGUI.PropertyField(fieldRect, patternProperty);
                    break;
                case Condition.ConditionType.gridStationary:
                    EditorGUI.PropertyField(fieldRect, patternProperty);
                    EditorGUI.PropertyField(field2Rect, stationaryTilesProperty, true);
                    break;
                case Condition.ConditionType.spec:
                    EditorGUI.PropertyField(fieldRect, checkBoolProperty);
                    break;
                case Condition.ConditionType.playerCarryingItem:
                    EditorGUI.PropertyField(fieldRect, playerItemNameProperty);
                    break;
                case Condition.ConditionType.flag:
                case Condition.ConditionType.noFlag:
                    EditorGUI.PropertyField(fieldRect, flagNameProperty);
                    break;
            }
        }

        EditorGUI.EndProperty();
    }
}
