using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingRetriever : MonoBehaviour
{
    [SerializeField] private Settings setting;

    public void WriteSettingValue(object value)
    {
        SettingsManager.Setting(setting).SetCurrentValue(value);
    }

    public object ReadSettingValue()
    {
        return SettingsManager.Setting(setting).GetCurrentValue();
    }
}
