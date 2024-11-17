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

    private Action<string> showRebindingOverlayPanel;
    private Action hideRebindingOverlayPanel;

    private string localizedRebindingTemplate;

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

    private void OnEnable()
    {
        localizedRebindingTemplate = rebindingOverlayPanelText.text;
    }
    
    private void OnDisable()
    {
        rebindingOverlayPanelText.text = localizedRebindingTemplate;
    }

    private void ShowRebindingPanelForCurrentRebindOperation(string localizedName)
    {
        rebindingOverlayPanelText.text = IDialogueTableProvider.Interpolate(
            localizedRebindingTemplate,
            new() {
                {"control", localizedName}
            }
        );
        rebindingOverlayPanel.SetActive(true);
    }
}
