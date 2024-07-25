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

    private int timesControlsChangedRecently;
    private Coroutine timesControlsChangedCoroutine;
    private bool controlsWarningEnabled;
    private Coroutine controlsWarningCoroutine;

    public void OnControlsChanged()
    {
        //string newControlScheme = GetCurrentControlScheme();
        string newControlScheme = playerInput.currentControlScheme;
        HandleControllerWarnings();
        Debug.Log("[Input] Control Scheme changed to: " + newControlScheme);
        Controls.CurrentControlScheme = newControlScheme;

        OnControlSchemeChanged?.Invoke();
    }

    private void HandleControllerWarnings()
    {
        timesControlsChangedRecently += 1;
        UIControllerWarning.SetWarningTextEnabled(controlsWarningEnabled);

        if (timesControlsChangedCoroutine != null)
        {
            StopCoroutine(timesControlsChangedCoroutine);
            timesControlsChangedCoroutine = null;
        }
        if (controlsWarningCoroutine != null)
        {
            StopCoroutine(controlsWarningCoroutine);
            controlsWarningCoroutine = null;
        }

        timesControlsChangedCoroutine = CoroutineUtils.ExecuteAfterDelay(
            () => {
                timesControlsChangedRecently = 0;
                timesControlsChangedCoroutine = null;
            },
            this,
            2
        );

        if (timesControlsChangedRecently > 4)
        {
            controlsWarningEnabled = true;
            UIControllerWarning.SetWarningTextEnabled(controlsWarningEnabled);

            controlsWarningCoroutine = CoroutineUtils.ExecuteAfterDelay(
                () => {
                    controlsWarningEnabled = false;
                    UIControllerWarning.SetWarningTextEnabled(controlsWarningEnabled);
                    controlsWarningCoroutine = null;
                },
                this,
                4
            );
        }
    }
}
