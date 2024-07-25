
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Obsolete]
public class AdvancedOptionsPanel : MonoBehaviour
{
    [SerializeField] private Slider screenShakeSlider;
    [SerializeField] private Toggle bigTextToggle;
    [SerializeField] private Toggle highContrastTextToggle;
    [SerializeField] private Toggle hideCursorToggle;
    [SerializeField] private Toggle miniPlayerIconToggle;
    [SerializeField] private Toggle autoMoveToggle;
    [SerializeField] private Toggle colorblindToggle;
    [SerializeField] private Toggle devConsoleToggle;

    private void Awake()
    {
        screenShakeSlider.onValueChanged.AddListener((float value) => { UpdateScreenShake(); });
        bigTextToggle.onValueChanged.AddListener((bool value) => { UpdateBigText(); });
        highContrastTextToggle.onValueChanged.AddListener((bool value) => { UpdateHighContrastText(); });
        hideCursorToggle.onValueChanged.AddListener((bool value) => { UpdateHideCursor(); });
        miniPlayerIconToggle.onValueChanged.AddListener((bool value) => { UpdateMiniPlayerIcon(); });
        autoMoveToggle.onValueChanged.AddListener((bool value) => { UpdateAutoMove(); });
        colorblindToggle.onValueChanged.AddListener((bool value) => { UpdateColorblind(); });
        devConsoleToggle.onValueChanged.AddListener((bool value) => { UpdateDevConsole(); });
    }

    private void OnEnable()
    {
        screenShakeSlider.value = SettingsManager.Setting<float>(Settings.ScreenShake).CurrentValue;
        bigTextToggle.isOn = SettingsManager.Setting<bool>(Settings.BigTextEnabled).CurrentValue;
        highContrastTextToggle.isOn = SettingsManager.Setting<bool>(Settings.HighContrastTextEnabled).CurrentValue;
        hideCursorToggle.isOn = SettingsManager.Setting<bool>(Settings.HideCursor).CurrentValue;
        miniPlayerIconToggle.isOn = SettingsManager.Setting<bool>(Settings.MiniPlayerIcon).CurrentValue;
        autoMoveToggle.isOn = SettingsManager.Setting<bool>(Settings.AutoMove).CurrentValue;
        colorblindToggle.isOn = SettingsManager.Setting<bool>(Settings.Colorblind).CurrentValue;
        devConsoleToggle.isOn = SettingsManager.Setting<bool>(Settings.DevConsole).CurrentValue;
    }

    public void UpdateScreenShake()
    {
        SettingsManager.Setting<float>(Settings.ScreenShake).CurrentValue = screenShakeSlider.value;
    }

    public void UpdateBigText()
    {
        // By the word of our noble lord, Boomo, long may he reign, these two lines must remain commented out
        //DialogueManager.highContrastMode = value;
        //DialogueManager.doubleSizeMode = value;

        SettingsManager.Setting<bool>(Settings.BigTextEnabled).CurrentValue = bigTextToggle.isOn;
    }

    public void UpdateHighContrastText()
    {
        SettingsManager.Setting<bool>(Settings.HighContrastTextEnabled).CurrentValue = highContrastTextToggle.isOn;
    }

    public void UpdateHideCursor()
    {
        SettingsManager.Setting<bool>(Settings.HideCursor).CurrentValue = hideCursorToggle.isOn;
    }

    public void UpdateMiniPlayerIcon()
    {
        SettingsManager.Setting<bool>(Settings.MiniPlayerIcon).CurrentValue = miniPlayerIconToggle.isOn;
    }

    public void UpdateAutoMove()
    {
        SettingsManager.Setting<bool>(Settings.AutoMove).CurrentValue = autoMoveToggle.isOn;
    }

    public void UpdateColorblind()
    {
        SettingsManager.Setting<bool>(Settings.Colorblind).CurrentValue = colorblindToggle.isOn;
    }

    public void UpdateDevConsole()
    {
        SettingsManager.Setting<bool>(Settings.DevConsole).CurrentValue = devConsoleToggle.isOn;
    }
}
