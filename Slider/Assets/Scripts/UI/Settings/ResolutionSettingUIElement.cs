using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResolutionSettingUIElement : SettingUIElement<int>
{
    private readonly Dictionary<int, Resolution> resolutions = new Dictionary<int, Resolution>()
    {
        { 0, new Resolution(1280, 720) },
        { 1, new Resolution(1280, 800) },
        { 2, new Resolution(1366, 768) },
        { 3, new Resolution(1920, 1080) },
        { 4, new Resolution(2560, 1440) },
        { 5, new Resolution(3840, 2160) }
    };

    protected override void OnEnable()
    {
        Resolution settingValue = (Resolution)settingRetriever.ReadSettingValue();
        int dropdownIndex = resolutions.ToList().First(pair => pair.Value == settingValue).Key;
        onValueLoad?.Invoke(dropdownIndex);
    }
}
