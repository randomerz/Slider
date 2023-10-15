using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenuPlayer : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private TMP_InputField newSaveNameInputField;

    public void OnControlsChanged()
    {
        //string newControlScheme = GetCurrentControlScheme();
        string newControlScheme = playerInput.currentControlScheme;
        Debug.Log("Control Scheme changed to: " + newControlScheme);
        Controls.CurrentControlScheme = newControlScheme;

        if (newControlScheme == Controls.CONTROL_SCHEME_KEYBOARD_MOUSE)
        {
            newSaveNameInputField.Select();
        }
    }
}
