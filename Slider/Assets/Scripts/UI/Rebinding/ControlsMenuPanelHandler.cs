using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEngine.UI;

/// <summary>
/// Handles switching between the various panels available in the controls menu and 
/// selecting the correct one to show when the menu is open based on the current control scheme. 
/// Use <see cref="SetCurrentPanel(int)"/> to change to a new panel.
/// </summary>
public class ControlsMenuPanelHandler : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private Selectable[] buttons;

    public const int KEYBOARD_PANEL = 0;
    public const int CONTROLLER_PANEL = 1;

    private void OnEnable()
    {
        bool currentControlSchemeIsController = Controls.CurrentControlScheme == Controls.CONTROL_SCHEME_CONTROLLER;
        int currentPanel = currentControlSchemeIsController ? CONTROLLER_PANEL : KEYBOARD_PANEL;

        SetCurrentPanel(currentPanel);
        UINavigationManager.SetCurrentSelectable(buttons[currentPanel]);
    }

    public void SetCurrentPanel(int newPanelIndex)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == newPanelIndex);
        }
    }
}
