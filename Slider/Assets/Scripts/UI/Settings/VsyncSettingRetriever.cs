using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VsyncSettingRetriever : AbstractSettingRetriever
{
    public override object ReadSettingValue()
    {
        return ((int) SettingsManager.Setting(Settings.Vsync).GetCurrentValue()) == GraphicsSettingsManager.VSYNC_ENABLED;
    }

    public override void WriteSettingValue(object value)
    {
        bool vsyncEnabled = (bool) value;
        SettingsManager.Setting(Settings.Vsync).SetCurrentValue(vsyncEnabled ? GraphicsSettingsManager.VSYNC_ENABLED : GraphicsSettingsManager.VSYNC_DISABLED);
    }
}
