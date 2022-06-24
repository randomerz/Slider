using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to persistent user settings, particularly for volume.
/// Use the public properties (SFXVolume, MusicVolume, etc) to adjust the values, which
/// actually updates them in the process. Those values are also written to PlayerPrefs
/// to persist between play sessions and loaded by the SettingsManager at the start of the game.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    private static SettingsManager _instance;

    private Settings currentSettings;

    // Start is called before the first frame update
    void Awake()
    {
        _instance = this;
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
    public static bool BigTextEnabled
    {
        get => _instance.currentSettings.bigTextEnabled;
        set
        {
            // Big text doesn't actually exist, but when it does... we'll be ready.
            _instance.currentSettings.bigTextEnabled = value;
            WriteCurrentSettingsToPlayerPrefs();
        }
    }
    public static float ScreenShake
    {
        get => _instance.currentSettings.screenShake;
        set
        {
            _instance.currentSettings.screenShake = value;
            WriteCurrentSettingsToPlayerPrefs();
        }
    }
    public static bool AutoMove
    {
        get => _instance.currentSettings.autoMove || _instance.currentSettings.forceAutoMove;
        set
        {
            _instance.currentSettings.autoMove = value;
            if (!value) _instance.currentSettings.forceAutoMove = false;

            WriteCurrentSettingsToPlayerPrefs();
        }
    }
    public static bool ForceAutoMove
    {
        get => _instance.currentSettings.forceAutoMove;
        set
        {
            _instance.currentSettings.forceAutoMove = value;

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
    public bool autoMove;
    public bool forceAutoMove;

    /// <summary>
    /// Returns an instance of Settings with volumes set to 50% and big text disabled.
    /// </summary>
    /// <returns></returns>
    public static Settings GetDefaultSettings()
    {
        return new Settings(0.5f, 0.5f, 1.0f, false, false);
    }

    public Settings(float sfxVolume, float musicVolume, float screenShake, bool bigTextEnabled, bool autoMove)
    {
        this.sfxVolume = sfxVolume;
        this.musicVolume = musicVolume;
        this.screenShake = screenShake;
        this.bigTextEnabled = bigTextEnabled;
        this.autoMove = autoMove;
        this.forceAutoMove = autoMove;
    }
}
