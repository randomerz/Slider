
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        screenShakeSlider.value = SettingsManager.ScreenShake;
        bigTextToggle.isOn = SettingsManager.BigTextEnabled;
        highContrastTextToggle.isOn = SettingsManager.HighContrastTextEnabled;
        hideCursorToggle.isOn = SettingsManager.HideCursor;
        miniPlayerIconToggle.isOn = SettingsManager.MiniPlayerIcon;
        autoMoveToggle.isOn = SettingsManager.AutoMove;
        colorblindToggle.isOn = SettingsManager.Colorblind;
        devConsoleToggle.isOn = SettingsManager.DevConsole;
    }

    public void UpdateScreenShake()
    {
        SettingsManager.ScreenShake = screenShakeSlider.value;
    }

    public void UpdateBigText()
    {
        // By the word of our noble lord, Boomo, long may he reign, these two lines must remain commented out
        //DialogueManager.highContrastMode = value;
        //DialogueManager.doubleSizeMode = value;

        SettingsManager.BigTextEnabled = bigTextToggle.isOn;
    }

    public void UpdateHighContrastText()
    {
        SettingsManager.HighContrastTextEnabled = highContrastTextToggle.isOn;
    }

    public void UpdateHideCursor()
    {
        SettingsManager.HideCursor = hideCursorToggle.isOn;
    }

    public void UpdateMiniPlayerIcon()
    {
        SettingsManager.MiniPlayerIcon = miniPlayerIconToggle.isOn;
    }

    public void UpdateAutoMove()
    {
        SettingsManager.AutoMove = autoMoveToggle.isOn;
    }

    public void UpdateColorblind()
    {
        SettingsManager.Colorblind = colorblindToggle.isOn;
    }

    public void UpdateDevConsole()
    {
        SettingsManager.DevConsole = devConsoleToggle.isOn;
    }
}
