// using UnityEngine;
// using UnityEditor;
// using UnityEngine.UIElements;
// using UnityEditor.UIElements;

// [CustomPropertyDrawer(typeof(DialogueData))]
// public class DialogueDataDrawerUIE : PropertyDrawer
// {
    
//     public override VisualElement CreatePropertyGUI(SerializedProperty property)
//     {
//         // Create property container element.
//         var container = new VisualElement();

//         // Create property fields.
//         var dialogueField = new PropertyField(property.FindPropertyRelative("dialogue"));
//         var delayAfterFinishedField = new PropertyField(property.FindPropertyRelative("delayAfterFinishedTyping"));
//         var waitUntilPlayerActionField = new PropertyField(property.FindPropertyRelative("waitUntilPlayerAction"));
//         var advanceDialogueManuallyField = new PropertyField(property.FindPropertyRelative("advanceDialogueManually"));
//         var doNotRepeatEventsField = new PropertyField(property.FindPropertyRelative("doNotRepeatEvents"));
//         var dontInterruptField = new PropertyField(property.FindPropertyRelative("dontInterrupt"));
//         var onDialogueStartField = new PropertyField(property.FindPropertyRelative("onDialogueStart"));
//         var onDialogueEndField = new PropertyField(property.FindPropertyRelative("onDialogueEnd"));

//         // Add fields to the container.
//         container.Add(dialogueField);
//         container.Add(delayAfterFinishedField);
//         container.Add(waitUntilPlayerActionField);
//         container.Add(advanceDialogueManuallyField);
//         container.Add(doNotRepeatEventsField);
//         container.Add(dontInterruptField);
//         container.Add(onDialogueStartField);
//         container.Add(onDialogueEndField);

//         return container;
//     }
// }
