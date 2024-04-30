using System;
using UnityEngine;

public interface ISetting
{
    public void LoadFromPlayerPrefs();
    public void SaveToPlayerPrefs();
    public void SetCurrentValue(object value);
    public object GetCurrentValue();
    public void ResetToDefaultValue();
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
            OnValueChanged?.Invoke(value);
        }
    }

    public readonly T DefaultValue;
    public readonly string PlayerPrefsKey;
    public Action<T> OnValueChanged;

    /// <param name="playerPrefsKey"></param>
    /// <param name="defaultValue">If the setting value is not found in PlayerPrefs when loading is attempted, this value will be used</param>
    /// <param name="onValueChanged">Action to execute whenever the value of this setting is changed, including when loaded from PlayerPrefs</param>
    public Setting(string playerPrefsKey, T defaultValue, Action<T> onValueChanged = null)
    {
        DefaultValue = defaultValue;
        CurrentValue = defaultValue;
        PlayerPrefsKey = playerPrefsKey;
        OnValueChanged = onValueChanged;
    }

    public void LoadFromPlayerPrefs()
    {
        T valueFromPlayerPrefs = DefaultValue;
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            string jsonFromPlayerPrefs = PlayerPrefs.GetString(PlayerPrefsKey);
            valueFromPlayerPrefs = JsonUtility.FromJson<SerializableSettingValue<T>>(jsonFromPlayerPrefs).value;
        }

        _currentValue = valueFromPlayerPrefs;
        OnValueChanged?.Invoke(valueFromPlayerPrefs);
    }

    public void SaveToPlayerPrefs()
    {
        T valueToSave = _currentValue != null ? _currentValue : DefaultValue;
        PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(new SerializableSettingValue<T>(valueToSave)));
    }

    public void SetCurrentValue(object value)
    {
        CurrentValue = (T)value;
    }

    public object GetCurrentValue()
    {
        return CurrentValue;
    }

    public void ResetToDefaultValue()
    {
        CurrentValue = DefaultValue;
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
