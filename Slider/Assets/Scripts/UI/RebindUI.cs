// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.InputSystem;

// public class RebindUI : MonoBehaviour {
//     [SerializeField]
//     private InputActionReference inputActionReference;

//     [SerializeField]
//     private bool excludeMouse = true;

//     [Range(0,10)]
//     [SerializeField]
//     private int selectedBinding;
//     [SerializeField]
//     private InputBinding.DisplayStringOptions displayStringOptions;
//     [Header("Binding Info - DO NOT EDIT")]
//     [SerializeField]    
//     private InputBinding inputBinding;
//     private int bindingIndex;

//     private string actionName;

//     [Header("UI Fields")]   
//     [SerializeField]
//     private Text actionText;
//     [SerializeField]
//     private Button rebindButton;
//     [SerializeField]
//     private Text rebindText;
//     [SerializeField]
//     private Button resetButton;

//     private void OnEnable() 
//     {
//         rebindButton.onClick.AddListener(() => DoRebind());
//         rebindButton.onClick.AddListener(() => ResetBinding());    

//         if (inputActionReference != null) 
//         {
//             GetBindingInfo();
//             UpdateUI();
//         }
//         InputManager.rebindComplete += UpdateUI;
//         InputManager.rebindCanceled += UpdateUI;
//     }

//     private void OnDisable() 
//     {
//         InputManager.rebindComplete -= UpdateUI;
//         InputManager.rebindCanceled -= UpdateUI;
//     }

//     private void OnValidate() 
//     {
//         if (inputActionReference == null) 
//         {
//             return;
//         }
//         GetBindingInfo();
//         UpdateUI();
//     }

//     private void GetBindingInfo() 
//     {
//         if (inputActionReference.action != null) 
//         {
//             actionName = inputActionReference.action.name;
//         }
//         if (inputActionReference.action.bindings.Count > selectedBinding) 
//         {
//             inputBinding = inputActionReference.action.bindings[selectedBinding];
//             bindingIndex = selectedBinding;
//         }
//     }

//     private void UpdateUI() 
//     {
//         if (actionText != null) 
//         {
//             actionText.text = actionName;
//         }
//         if (rebindText != null) 
//         {
//             if (Application.isPlaying) 
//             {
//                 //grab info from InputManager
//                 rebindText.text = InputManager.GetBindingName(actionName, bindingIndex);
//             } 
//             else 
//             {
//                 rebindText.text = inputActionReference.action.GetBindingDisplayString(bindingIndex);
//             }
//         }
//     } 

//     private void DoRebind() 
//     {
//         InputManager.StartRebind(actionName, bindingIndex, rebindText, excludeMouse);
//     }

//     private void ResetBinding()
//     {
//         InputManager.ResetBinding(actionName, bindingIndex);
//         UpdateUI();
//     }
// }

