using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


[CustomPropertyDrawer(typeof(Conditionals.Condition))]
public class ConditionalsEditor : PropertyDrawer
{
    // The enum field that will determine what variables to display in the Inspector

    //This is what actually makes the editor
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var container = new VisualElement();

        var categoryToDisplay = (Conditionals.Condition.ConditionType) property.FindPropertyRelative("type").enumValueIndex;

        var itemField = new PropertyField(property.FindPropertyRelative("item"));
        var patternField = new PropertyField(property.FindPropertyRelative("pattern"));
        var checkBoolField = new PropertyField(property.FindPropertyRelative("checkBool"));

        container.Add(new PropertyField(property.FindPropertyRelative("type")));

        // Check the value of the enum and display variables based on it
        switch (categoryToDisplay)
        {
            case Conditionals.Condition.ConditionType.item:
                container.Add(itemField);
                break;
            case Conditionals.Condition.ConditionType.grid:
                container.Add(patternField);
                break;
            case Conditionals.Condition.ConditionType.spec:
                container.Add(checkBoolField);
                break;
        }

        return container;
    }
}

