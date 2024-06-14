using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// <para>
/// This class handles saving and loading settings to and from PlayerPrefs.
/// </para>
/// <para>
/// To access a particular setting, use <see cref="SettingsManager.Setting{T}(Settings)"/> (e.g. SettingsManager.Setting&lt;bool&gt;(Settings.HideCursor).CurrentValue )
/// </para>
/// <para>
/// To add a new setting:
/// 1. Add the setting to the Settings enum.
/// 2. Add the setting's PlayerPrefsKey to SettingsMethods.PlayerPrefsKey.
/// 3. Add a call to RegisterAndLoadSetting in an Awake method of any class to set the properties of the setting.
/// </para>
/// </summary>
public class SettingsManager : MonoBehaviour
{
    private static readonly Dictionary<Settings, ISetting> settings = new();
    public static readonly Dictionary<Settings, Action<object>> OnSettingChanged = new();

    void Awake()
    {
        RegisterAndLoadSetting(Settings.MasterVolume,
            defaultValue: 0.5f,
            onValueChanged: value => AudioManager.SetMasterVolume(value)
        );
        RegisterAndLoadSetting(Settings.SFXVolume,
            defaultValue: 0.5f,
            onValueChanged: value => AudioManager.SetSFXVolume(value)
        );
        RegisterAndLoadSetting(Settings.MusicVolume,
            defaultValue: 0.5f,
            onValueChanged: value => AudioManager.SetMusicVolume(value)
        );
        RegisterAndLoadSetting(Settings.AmbienceVolume,
            defaultValue: 0.5f,
            onValueChanged: value => AudioManager.SetAmbienceVolume(value)
        );
        RegisterAndLoadSetting(Settings.ScreenShake,
            defaultValue: 0.5f
        );
        RegisterAndLoadSetting(Settings.BigTextEnabled,
            defaultValue: false
        );
        RegisterAndLoadSetting(Settings.HighContrastTextEnabled,
            defaultValue: false
        );
        RegisterAndLoadSetting(Settings.PixelFontEnabled,
            defaultValue: true
        );
        RegisterAndLoadSetting(Settings.Colorblind,
            defaultValue: false
        );
        RegisterAndLoadSetting(Settings.DevConsole,
            defaultValue: false
        );
        RegisterAndLoadSetting(Settings.HideCursor,
            defaultValue: true
        );
        RegisterAndLoadSetting(Settings.MiniPlayerIcon,
            defaultValue: false,
            onValueChanged: (value) =>
            {
                Player.AddTrackerOnSettingsChange();
            }
        );
        // This is not currently used, but may be put back into the UI later
        RegisterAndLoadSetting(Settings.AutoMove,
            defaultValue: false,
            onValueChanged: (value) =>
            {
                /*if (!value && !GameUI.instance.isMenuScene && SaveSystem.Current != null) 
                    SaveSystem.Current.SetBool("forceAutoMove", false);*/
            }
        );
        RegisterAndLoadSetting(Settings.PlayAudioWhenUnfocused,
            defaultValue: false,
            onValueChanged: (value) => Application.runInBackground = value
        );
    }

    public static void RegisterAndLoadSetting<T>(Settings setting, T defaultValue, Action<T> onValueChanged = null)
    {
        Setting<T> newSetting = new Setting<T>
        (
            playerPrefsKey: setting.PlayerPrefsKey(),
            defaultValue: defaultValue,
            onValueChanged: onValueChanged
        );
        settings[setting] = newSetting;

        OnSettingChanged[setting] = new Action<object>((newSettingValue) => { });
        newSetting.OnValueChanged += newSettingValue => OnSettingChanged[setting]?.Invoke(newSettingValue);
        newSetting.LoadFromPlayerPrefs();
    }

    /// <summary>
    /// Retrieves the Setting data class for the specified Setting. This can be used to access properties such as the CurrentValue, DefaultValue, etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="setting"></param>
    /// <returns></returns>
    public static Setting<T> Setting<T>(Settings setting)
    {
        return (Setting<T>)settings[setting];
    }

    /// <summary>
    /// Retrieves the Setting data class for the specified Setting. This can be used to access properties such as the CurrentValue, DefaultValue, etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="setting"></param>
    /// <returns></returns>
    public static ISetting Setting(Settings setting)
    {
        return settings[setting];
    }

    public static void ResetAllSettingsToDefaults()
    {
        settings.Values.ToList().ForEach(setting => setting.ResetToDefaultValue());
    }
}
