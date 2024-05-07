using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SettingsManager;

public class GraphicsSettingsManager : MonoBehaviour
{
    public const int VSYNC_DISABLED = 0;
    public const int VSYNC_ENABLED = 1;
    public const int TARGET_FRAME_RATE_DISABLED = -1;

    private void Awake()
    {
        RegisterAndLoadSetting(Settings.ScreenMode,
            defaultValue: FullScreenMode.ExclusiveFullScreen,
            onValueChanged: (value) => Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, value)
        );
        RegisterAndLoadSetting(Settings.Resolution,
            defaultValue: new Resolution(1920, 1080),
            onValueChanged: (value) => Screen.SetResolution(value.width, value.height, Screen.fullScreenMode)
        );
        RegisterAndLoadSetting(Settings.Vsync,
            defaultValue: VSYNC_ENABLED,
            onValueChanged: (value) => QualitySettings.vSyncCount = value
        );
        RegisterAndLoadSetting(Settings.TargetFrameRate,
            defaultValue: TARGET_FRAME_RATE_DISABLED,
            onValueChanged: (value) => Application.targetFrameRate = value
        );
    }
}
