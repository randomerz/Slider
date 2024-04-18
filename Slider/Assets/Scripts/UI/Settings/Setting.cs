using System;
using UnityEngine;

// This is just to make it so we can make a Dictionary with Settings of different generic types
interface ISetting
{
    public void LoadFromPlayerPrefs();
    public void SaveToPlayerPrefs();
}

/// <summary>
/// Data class used by SettingsManager to track various user settings and save/load their values to/from PlayerPrefs.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Setting<T> : ISetting
{
    private T _currentValue;
    public T CurrentValue
    {
        get => _currentValue;
        set
        {
            _currentValue = value;
            SaveToPlayerPrefs();
            onValueChanged?.Invoke(value);
        }
    }
    public readonly T DefaultValue;
    public readonly string PlayerPrefsKey;
    private readonly Action<T> onValueChanged;

    /// <param name="playerPrefsKey"></param>
    /// <param name="defaultValue">If the setting value is not found in PlayerPrefs when loading is attempted, this value will be used</param>
    /// <param name="onValueChanged">Action to execute whenever the value of this setting is changed, including when loaded from PlayerPrefs</param>
    public Setting(string playerPrefsKey, T defaultValue, Action<T> onValueChanged = null)
    {
        DefaultValue = defaultValue;
        CurrentValue = defaultValue;
        PlayerPrefsKey = playerPrefsKey;
        this.onValueChanged = onValueChanged;
    }

    public void LoadFromPlayerPrefs()
    {
        T valueFromPlayerPrefs;
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            string jsonFromPlayerPrefs = PlayerPrefs.GetString(PlayerPrefsKey);
            valueFromPlayerPrefs = JsonUtility.FromJson<SerializableSettingValue<T>>(jsonFromPlayerPrefs).value;
        }
        else
        {
            valueFromPlayerPrefs = DefaultValue;
        }

        _currentValue = valueFromPlayerPrefs;
        onValueChanged?.Invoke(valueFromPlayerPrefs);
    }

    public void SaveToPlayerPrefs()
    {
        T valueToSave = _currentValue != null ? _currentValue : DefaultValue;
        PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(new SerializableSettingValue<T>(valueToSave)));
    }

    private class SerializableSettingValue<U>
    {
        public U value;

        public SerializableSettingValue(U value)
        {
            this.value = value;
        }
    }
}
