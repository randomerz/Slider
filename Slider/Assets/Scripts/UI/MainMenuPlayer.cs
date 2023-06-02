using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenuPlayer : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Controls controls;
    [SerializeField] private TMP_InputField newSaveNameInputField;

    public void OnControlsChanged()
    {
        //string newControlScheme = GetCurrentControlScheme();
        string newControlScheme = playerInput.currentControlScheme;
        Debug.Log("Control Scheme changed to: " + newControlScheme);
        controls.SetCurrentControlScheme(newControlScheme);

        if (newControlScheme == "Keyboard Mouse")
        {
            newSaveNameInputField.Select();
        }
    }
}
