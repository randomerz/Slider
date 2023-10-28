using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class MainMenuPlayer : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private TMP_InputField newSaveNameInputField;

    public static Action OnControlSchemeChanged;

    public void OnControlsChanged()
    {
        //string newControlScheme = GetCurrentControlScheme();
        string newControlScheme = playerInput.currentControlScheme;
        Debug.Log("[Input] Control Scheme changed to: " + newControlScheme);
        Controls.CurrentControlScheme = newControlScheme;

        OnControlSchemeChanged?.Invoke();
    }
}
