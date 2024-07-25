
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Obsolete]
public class MiscOptionsPanel : MonoBehaviour
{
    [SerializeField] private Toggle miniPlayerIconToggle;
    [SerializeField] private Toggle colorblindToggle;
    [SerializeField] private Toggle devConsoleToggle;

    private void Awake()
    {
        miniPlayerIconToggle.onValueChanged.AddListener((bool value) => { UpdateMiniPlayerIcon(); });
        colorblindToggle.onValueChanged.AddListener((bool value) => { UpdateColorblind(); });
        devConsoleToggle.onValueChanged.AddListener((bool value) => { UpdateDevConsole(); });
    }

    private void OnEnable()
    {
        miniPlayerIconToggle.isOn = SettingsManager.Setting<bool>(Settings.MiniPlayerIcon).CurrentValue;
        colorblindToggle.isOn = SettingsManager.Setting<bool>(Settings.Colorblind).CurrentValue;
        devConsoleToggle.isOn = SettingsManager.Setting<bool>(Settings.DevConsole).CurrentValue;
    }

    public void UpdateMiniPlayerIcon()
    {
        SettingsManager.Setting<bool>(Settings.MiniPlayerIcon).CurrentValue = miniPlayerIconToggle.isOn;
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
