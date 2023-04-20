using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuPlayer : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Controls controls;

    public void OnControlsChanged()
    {
        //string newControlScheme = GetCurrentControlScheme();
        string newControlScheme = playerInput.currentControlScheme;
        Debug.Log("Control Scheme changed to: " + newControlScheme);
        controls.SetCurrentControlScheme(newControlScheme);
    }
}
