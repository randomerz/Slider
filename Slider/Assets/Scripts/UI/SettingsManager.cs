using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to persistent user settings, particularly for volume.
/// Use the public properties (SFXVolume, MusicVolume, etc) to adjust the values, which
/// actually updates them in the process. Those values are also written to PlayerPrefs
/// to persist between play sessions and loaded by the SettingsManager at the start of the game.
/// </summary>
public class SettingsManager : Singleton<SettingsManager>
{
    private Settings currentSettings;

    // Start is called before the first frame update
    void Awake()
    {
        InitializeSingleton(overrideExistingInstanceWith: this);
        LoadSettings();
    }

    // Public properties for getting/setting all of our settings
    // Changing these also modifies the actual values in-game and writes the new value to PlayerPrefs
    public static float SFXVolume
    {
        get => _instance.currentSettings.sfxVolume;
        set
        {
            _instance.currentSettings.sfxVolume = value;
            AudioManager.SetSFXVolume(value);
            WriteCurrentSettingsToPlayerPrefs();
        }
    }
    public static float MusicVolume
    {
        get => _instance.currentSettings.musicVolume;
        set
        {
            _instance.currentSettings.musicVolume = value;
            AudioManager.SetMusicVolume(value);
            WriteCurrentSettingsToPlayerPrefs();
        }
    }

    // Advanced Options
    public static float ScreenShake
    {
        get => _instance.currentSettings.screenShake;
        set
        {
            _instance.currentSettings.screenShake = value;
            WriteCurrentSettingsToPlayerPrefs();
        }
    }
    public static bool BigTextEnabled
    {
        get => _instance.currentSettings.bigTextEnabled;
        set
        {
            _instance.currentSettings.bigTextEnabled = value;
            WriteCurrentSettingsToPlayerPrefs();
        }
    }
    public static bool HighContrastTextEnabled
    {
        get => _instance.currentSettings.highContrastTextEnabled;
        set
        {
            _instance.currentSettings.highContrastTextEnabled = value;
            WriteCurrentSettingsToPlayerPrefs();
        }
    }
    public static bool HideCursor
    {
        get => _instance.currentSettings.hideCursor;
        set
        {
            _instance.currentSettings.hideCursor = value;
            WriteCurrentSettingsToPlayerPrefs();
        }
    }
    public static bool MiniPlayerIcon
    {
        get => _instance.currentSettings.miniPlayerIcon;
        set
        {
            _instance.currentSettings.miniPlayerIcon = value;
            WriteCurrentSettingsToPlayerPrefs();
        }
    }
    public static bool AutoMove
    {
        get => _instance.currentSettings.autoMove || (!GameUI.instance.isMenuScene && SaveSystem.Current.GetBool("forceAutoMove"));
        set
        {
            _instance.currentSettings.autoMove = value;
            if (!value && !GameUI.instance.isMenuScene) 
                SaveSystem.Current.SetBool("forceAutoMove", false);

            WriteCurrentSettingsToPlayerPrefs();
        }
    }
    public static bool Colorblind
    {
        get => _instance.currentSettings.colorblindMode;
        set
        {
            _instance.currentSettings.colorblindMode = value;
            WriteCurrentSettingsToPlayerPrefs();
        }
    }

    // Synonymous with Debug Mode
    public static bool DevConsole
    {
        get => _instance.currentSettings.devConsoleEnabled;
        set
        {
            _instance.currentSettings.devConsoleEnabled = value;
            WriteCurrentSettingsToPlayerPrefs();
        }
    }



    /// <summary>
    /// Call this whenever we update something in the settings so that 
    /// we always keep the latest in PlayerPrefs. Might not be the most
    /// efficient, but it works.
    /// </summary>
    public static void WriteCurrentSettingsToPlayerPrefs()
    {
        string settings = JsonUtility.ToJson(_instance.currentSettings);
        PlayerPrefs.SetString("settings", settings);
    }

    /// <summary>
    /// Searches PlayerPrefs for "settings" and, if a string is found,
    /// tries to parse the (hopefully) JSON into a Settings struct.
    /// </summary>
    public static void LoadSettings()
    {
        string settings = PlayerPrefs.GetString("settings");
        if (string.IsNullOrEmpty(settings))
        {
            _instance.currentSettings = Settings.GetDefaultSettings();
        }
        else
        {
            _instance.currentSettings = JsonUtility.FromJson<Settings>(PlayerPrefs.GetString("settings"));
        }
        AudioManager.SetMusicVolume(_instance.currentSettings.musicVolume);
        AudioManager.SetSFXVolume(_instance.currentSettings.sfxVolume);
    }
}

/// <summary>
/// Used in SettingsManager to store the current settings and write them to/from JSON 
/// for storage/retrieval from PlayerPrefs.
/// </summary>
[System.Serializable]
struct Settings
{
    public float sfxVolume;
    public float musicVolume;

    public float screenShake;
    public bool bigTextEnabled;
    public bool highContrastTextEnabled;
    public bool hideCursor;
    public bool miniPlayerIcon;
    public bool autoMove;
    public bool forceAutoMove;
    public bool colorblindMode;
    public bool devConsoleEnabled;

    /// <summary>
    /// Returns an instance of Settings with volumes set to 50% and big text disabled.
    /// </summary>
    /// <returns></returns>
    public static Settings GetDefaultSettings()
    {
        return new Settings(
            sfxVolume: 0.5f, 
            musicVolume: 0.5f, 
            screenShake: 0.5f, 
            bigTextEnabled: false, 
            highContrastTextEnabled: false, 
            hideCursor: true, 
            miniPlayerIcon: false,
            autoMove: false,
            devConsoleEnabled: false
        );
    }

    public Settings(
        float sfxVolume, 
        float musicVolume, 
        float screenShake, 
        bool bigTextEnabled, 
        bool highContrastTextEnabled, 
        bool hideCursor, 
        bool miniPlayerIcon, 
        bool autoMove,
        bool devConsoleEnabled
    ) {
        this.sfxVolume = sfxVolume;
        this.musicVolume = musicVolume;
        this.screenShake = screenShake;
        this.bigTextEnabled = bigTextEnabled;
        this.highContrastTextEnabled = highContrastTextEnabled;
        this.hideCursor = hideCursor;
        this.miniPlayerIcon = miniPlayerIcon;
        this.autoMove = autoMove;
        this.devConsoleEnabled = devConsoleEnabled;
        this.forceAutoMove = autoMove;
        this.colorblindMode = true;
    }
}
