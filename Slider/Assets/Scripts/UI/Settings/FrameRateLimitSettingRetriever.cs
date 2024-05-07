using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FrameRateLimitSettingRetriever : AbstractSettingRetriever
{
    // This list has to match the order in the FPS setting dropdown
    private static readonly Dictionary<int, int> dropdownIndexToFrameRateLimit = new Dictionary<int, int>()
    {
        { 0, 30 },
        { 1, 60 },
        { 2, 75 },
        { 3, 120 },
        { 4, 144 },
        { 5, 244},
        { 6, GraphicsSettingsManager.TARGET_FRAME_RATE_DISABLED }
    };
    private static readonly Dictionary<int, int> frameRateLimitToDropdownIndex = dropdownIndexToFrameRateLimit.ToDictionary((i) => i.Value, (i) => i.Key);

    public override object ReadSettingValue()
    {
        int currentFrameRateLimit = (int)SettingsManager.Setting(Settings.TargetFrameRate).GetCurrentValue();
        return frameRateLimitToDropdownIndex[currentFrameRateLimit];
    }

    public override void WriteSettingValue(object value)
    {
        SettingsManager.Setting(Settings.TargetFrameRate).SetCurrentValue(dropdownIndexToFrameRateLimit[(int)value]);
    }
}
