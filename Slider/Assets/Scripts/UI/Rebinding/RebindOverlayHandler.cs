using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles enabling and disabling the overlay which covers the controls menu while a rebind operation is in progress.
/// Also sets the text of that overlay based on which action is being rebinded.
/// </summary>
public class RebindOverlayHandler : MonoBehaviour
{
    [SerializeField] private GameObject rebindingOverlayPanel;
    [SerializeField] private TextMeshProUGUI rebindingOverlayPanelText;

    private void Awake()
    {
        InputRebinding.OnRebindStarted += ShowRebindingPanelForCurrentRebindOperation;
        InputRebinding.OnRebindCompleted += () => rebindingOverlayPanel.SetActive(false);
        rebindingOverlayPanel.SetActive(false);
    }

    private void ShowRebindingPanelForCurrentRebindOperation(Control control)
    {
        string controlDisplayString = Controls.ControlDisplayString(control)
                                              .ToUpper();
        rebindingOverlayPanelText.text = $"REBINDING {controlDisplayString}...";
        rebindingOverlayPanel.SetActive(true);
    }
}
