// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.InputSystem;

// public class InputManager : MonoBehaviour 
// {
//     public static InputSettings inputActions;
//     private void Awake() 
//     {
//         if (inputActions == null) 
//         {
//             inputActions = new InputSettings();
//         }
//     }

//     public static void StartRebind(string actionName, int bindingIndex, Text statusText, bool excludeMouse) 
//     {
//         InputAction action = inputActions.asset.FindAction(actionName);
//         if (action == null || action.bindings.Count <= bindingIndex) 
//         {
//             Debug.Log("Couldn't find action or binding");
//             return;
//         }

//         if (action.bindings[bindingIndex].isComposite) 
//         {
//             var firstPartIndex = bindingIndex + 1;
//             if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isComposite) 
//             {
//                 DoRebind(action, bindingIndex, statusText, true, excludeMouse);
//             }
//         }
//         else 
//         {
//             DoRebind(action, bindingIndex, statusText, false, excludeMouse);
//         }
//     }

//     private static void DoRebind(InputAction actionToRebind, int bindingIndex, Text statusText, bool allCompositeParts, bool excludeMouse) 
//     {
//         if (actionToRebind == null || bindingIndex < 0) 
//         {
//             return;
//         }
//         statusText.text = $"Press a {actionToRebind.expectedControlType}";

//         actionToRebind.Disable();

//         var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex);
        
//         rebind.OnComplete(operation => 
//         {
//                 actionToRebind.Enable();
//                 operation.Dispose();

//                 if (allCompositeParts) 
//                 {
//                     var nextBindingIndex = bindingIndex + 1;
//                     if (nextBindingIndex < actionToRebind.bindings.Count && actionToRebind.bindings[bindingIndex].isComposite) 
//                     {
//                         DoRebind(actionToRebind, nextBindingIndex, statusText, allCompositeParts);
//                     }
//                 }
//         });

//         rebind.OnCancel(operation => 
//         {
//             actionToRebind.Enable();
//             operation.Dispose();

//             rebindCanceled?.Invoke();
//         });

//         rebind.WithCancelingThrough("<Keyboard>/escape");

//         if(excludeMouse)
//         {   
//             rebind.WithControlsExcluding("Mouse");
//         }
//         rebindStarted?.Invoke(actionToRebind, bindingIndex);
//         rebind.Start(); //actually starts the rebinding process
//     }

//     public static string GetBindingName(string actionName, int bindingIndex) 
//     {
//         if (inputActions == null) 
//         {
//             inputActions = new InputSettings();
//         }
//         InputAction action = inputActions.asset.FindAction(actionName);
//         return action.GetBindingDisplayString(bindingIndex);
//     }

//     public static void ResetBinding(string actionName, int bindingIndex) 
//     {
//         InputAction action = inputActions.asset.FindAction(actionName);

//         if (action == null || action.bindins.Count <= bindingIndex) 
//         {
//             Debug.Log("Could not find action or binding");
//             return;
//         }

//         if (action.bindings[bindingIndex].isComposite) 
//         {
//             for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++) 
//             {
//                 action.RemoveBindingOverride(i);
//             }
//         }
//         else {
//             action.RemoveBindingOverride(bindingIndex);
//         }
//     }
// }