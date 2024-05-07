using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.XInput;
using System.Linq;

/// <summary>
/// Handles switching between the various panels available in the controls menu and 
/// selecting the correct one to show when the menu is open based on the current control scheme. 
/// Use <see cref="SetCurrentPanel(int)"/> to change to a new panel.
/// </summary>
public class ControlsMenuPanelHandler : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private TextMeshProUGUI titleText;
    
    public const int KEYBOARD_PANEL = 0;
    public const int CONTROLLER_PANEL = 1;
    private string[] SCHEME_NAMES = {
        "Keyboard",
        "Controller"
    };

    private int currentPanel = 0;

    private void OnEnable()
    {
        bool currentControlSchemeIsController = Controls.CurrentControlScheme == Controls.CONTROL_SCHEME_CONTROLLER;
        currentPanel = currentControlSchemeIsController ? CONTROLLER_PANEL : KEYBOARD_PANEL;

        SetCurrentPanel(currentPanel);
    }

    public void SetCurrentPanel(int newPanelIndex)
    {
        currentPanel = newPanelIndex;
        titleText.text = SCHEME_NAMES[newPanelIndex];
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == newPanelIndex);
        }
    }

    public void MoveToNextPanel()
    {
        int newIndex = (currentPanel + 1) % panels.Length;
        SetCurrentPanel(newIndex);
    }

    public void MoveToPreviousPanel()
    {
        int newIndex = currentPanel - 1;
        if (newIndex < 0)
        {
            newIndex = panels.Length - 1;
        }
        SetCurrentPanel(newIndex);
    }

    public void SelectBestButtonInCurrentPanel()
    {
        SelectableSet currentSubMenuSelectableSet = panels[currentPanel].GetComponent<SelectableSet>();
        Selectable bestSelectableInCurrentMenu = currentSubMenuSelectableSet.Selectables.Where(selectable => selectable.IsInteractable()).First();
        CoroutineUtils.ExecuteAfterEndOfFrame(() => bestSelectableInCurrentMenu.Select(), this);

        Debug.Log(currentSubMenuSelectableSet.gameObject.name);
    }
}
