using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Handles enabling and disabling the overlay which covers the controls menu while a rebind operation is in progress.
/// Also sets the text of that overlay based on which action is being rebinded.
/// </summary>
public class RebindOverlayHandler : MonoBehaviour
{
    [SerializeField] private GameObject rebindingOverlayPanel;
    [SerializeField] private TextMeshProUGUI rebindingOverlayPanelText;

    private Action<Control> showRebindingOverlayPanel;
    private Action hideRebindingOverlayPanel;

    private void Awake()
    {
        showRebindingOverlayPanel = ShowRebindingPanelForCurrentRebindOperation;
        hideRebindingOverlayPanel = () => rebindingOverlayPanel.SetActive(false);

        InputRebinding.OnRebindStarted += showRebindingOverlayPanel;
        InputRebinding.OnRebindCompleted += hideRebindingOverlayPanel;
        rebindingOverlayPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        InputRebinding.OnRebindStarted -= showRebindingOverlayPanel;
        InputRebinding.OnRebindCompleted -= hideRebindingOverlayPanel;
    }

    private void ShowRebindingPanelForCurrentRebindOperation(Control control)
    {
        string controlDisplayString = Controls.ControlDisplayString(control)
                                              .ToUpper();
        rebindingOverlayPanelText.text = $"REBINDING {controlDisplayString}...";
        rebindingOverlayPanel.SetActive(true);
    }
}
