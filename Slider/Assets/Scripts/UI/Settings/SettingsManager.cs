using System;
using System.Collections.Generic;
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
    private static Dictionary<Settings, ISetting> settings = new();

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
            defaultValue: false
        );
        RegisterAndLoadSetting(Settings.AutoMove,
            defaultValue: false,
            onValueChanged: (value) => 
            {
                if (!value && !GameUI.instance.isMenuScene && SaveSystem.Current != null) 
                    SaveSystem.Current.SetBool("forceAutoMove", false);
            }
        );
        RegisterAndLoadSetting(Settings.ScreenMode,
            defaultValue: FullScreenMode.ExclusiveFullScreen,
            onValueChanged: (value) => Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, value)
        );
        RegisterAndLoadSetting(Settings.Resolution,
            defaultValue: new Resolution(1920, 1080),
            onValueChanged: (value) => Screen.SetResolution(value.width, value.height, Screen.fullScreenMode)
        );
        RegisterAndLoadSetting(Settings.Vsync,
            defaultValue: 1, // Vsync enabled
            onValueChanged: (value) => QualitySettings.vSyncCount = value
        );
        RegisterAndLoadSetting(Settings.TargetFrameRate,
            defaultValue: -1, // Target frame rate disabled
            onValueChanged: (value) => Application.targetFrameRate = value
        );
    }

    public static void RegisterAndLoadSetting<T>(Settings setting, T defaultValue, Action<T> onValueChanged = null)
    {
        settings[setting] = new Setting<T>
        (
            playerPrefsKey: setting.PlayerPrefsKey(),
            defaultValue: defaultValue, 
            onValueChanged: onValueChanged
        );
        settings[setting].LoadFromPlayerPrefs();
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
}

[System.Serializable]
public class Resolution
{
    public int width;
    public int height;

    public Resolution(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public static Resolution FromUnityStruct(UnityEngine.Resolution unityStruct)
    {
        return new Resolution(unityStruct.width, unityStruct.height);
    }
}
