using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingRetriever : AbstractSettingRetriever
{
    [SerializeField] private Settings setting;

    public override void WriteSettingValue(object value)
    {
        Debug.Log("writing setting value for setting [" + setting + "] to val [" + value + "]");
        SettingsManager.Setting(setting).SetCurrentValue(value);
    }

    public override object ReadSettingValue()
    {
        return SettingsManager.Setting(setting).GetCurrentValue();
    }
}
