using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResolutionSettingRetriever : AbstractSettingRetriever
{
    // This list has to match the order in the resolution setting dropdown
    private static readonly Dictionary<int, Resolution> dropdownIndexToResolution = new Dictionary<int, Resolution>()
    {
        { 0, new Resolution(1280, 720) },
        { 1, new Resolution(1280, 800) },
        { 2, new Resolution(1366, 768) },
        { 3, new Resolution(1920, 1080) },
        { 4, new Resolution(2560, 1440) },
        { 5, new Resolution(0, 0)},

    };
    private static readonly Dictionary<Resolution, int> resolutionToDropdownIndex = dropdownIndexToResolution.ToDictionary((i) => i.Value, (i) => i.Key);

    public override object ReadSettingValue()
    {
        Resolution currentResolution = (Resolution)SettingsManager.Setting(Settings.Resolution).GetCurrentValue();
        return resolutionToDropdownIndex[currentResolution];
    }

    public override void WriteSettingValue(object value)
    {
        SettingsManager.Setting(Settings.Resolution).SetCurrentValue(dropdownIndexToResolution[(int)value]);
    }
}
