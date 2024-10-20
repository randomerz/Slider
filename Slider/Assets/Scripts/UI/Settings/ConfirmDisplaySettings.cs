using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDisplaySettings : Singleton<ConfirmDisplaySettings>
{
    public Button applyChangesButton;
    public TextMeshProUGUI applyChangesText;
    public ResolutionSettingRetriever resolutionSettingsRetriever;
    public Toggle fullScreenToggle;

    private const float CONFIRMATION_TIME = 10;

    private string applyChangesOriginalText;
    private bool didConfirm;
    private Coroutine checkCoroutine;
    private System.Action currentOnFail;

    private void Awake()
    {
        InitializeSingleton(this);
    }

    private void Start()
    {
        applyChangesOriginalText = applyChangesText.text;
        SetConfirmButtonInteractable(false);
    }

    private void OnDisable()
    {
        if (checkCoroutine != null)
        {
            currentOnFail?.Invoke();
            FinishCheckingConditions();
            ResetButtonText();
        }
    }

    private void SetConfirmButtonInteractable(bool value)
    {
        applyChangesButton.interactable = value;
        applyChangesText.color = value ? GameSettings.white : GameSettings.darkGray;
    }

    public static void CheckSettingsConfirmed(System.Action onSuccess, System.Action onFail) =>
        _instance._CheckSettingsConfirmed(onSuccess, onFail);

    private void _CheckSettingsConfirmed(System.Action onSuccess, System.Action onFail)
    {
        didConfirm = false;
        StopAllCoroutines();
        checkCoroutine = StartCoroutine(CheckConditions(onSuccess, onFail));
    }


    private IEnumerator CheckConditions(System.Action onSuccess, System.Action onFail)
    {
        SetConfirmButtonInteractable(true);
        currentOnFail = onFail;

        float t = CONFIRMATION_TIME;
        while (t >= 0)
        {
            if (didConfirm)
            {
                onSuccess?.Invoke();
                FinishCheckingConditions();
                yield break;
            }

            t -= Time.unscaledDeltaTime;
            applyChangesText.text = $"{applyChangesOriginalText} ({(int)t + 1})";
            yield return null;
        }

        FinishCheckingConditions();
        onFail?.Invoke();
    }

    private void FinishCheckingConditions()
    {
        checkCoroutine = null;
        currentOnFail = null;
        SetConfirmButtonInteractable(false);
        ResetButtonText();
    }

    public void OnConfirmClicked()
    {
        didConfirm = true;
    }

    public static void RevertToSettings(bool isFullScreen, Resolution resolution)
    {
        _instance.fullScreenToggle.isOn = isFullScreen;
        _instance.resolutionSettingsRetriever.RestoreResolution(resolution);
    }

    private void ResetButtonText()
    {
        applyChangesText.text = applyChangesOriginalText;
    }
}
