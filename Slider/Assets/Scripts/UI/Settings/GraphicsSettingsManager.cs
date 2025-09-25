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

    private bool oldFullScreen;
    private Resolution oldResolution;
    public static bool skipConfirmingChanges;

    private void Awake()
    {
        skipConfirmingChanges = true;
        RegisterAndLoadSetting(Settings.FullScreen,
            defaultValue: true,
            onValueChanged: (value) => TryUpdateFullScreen(value)
        );
        RegisterAndLoadSetting(Settings.Resolution,
            defaultValue: new Resolution(0, 0),
            onValueChanged: (value) => TryUpdateResolution(value.width, value.height)
        );
        RegisterAndLoadSetting(Settings.Vsync,
            defaultValue: VSYNC_ENABLED,
            onValueChanged: (value) => QualitySettings.vSyncCount = value
        );
        RegisterAndLoadSetting(Settings.TargetFrameRate,
            defaultValue: TARGET_FRAME_RATE_DISABLED,
            onValueChanged: (value) => Application.targetFrameRate = value
        );

        oldFullScreen = (bool)Setting<bool>(Settings.FullScreen).GetCurrentValue();
        oldResolution = (Resolution)Setting<Resolution>(Settings.Resolution).GetCurrentValue();
        skipConfirmingChanges = false;
    }

    private void TryUpdateFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, GetFullScreenMode(isFullScreen));
    }

    private void TryUpdateResolution(int width, int height)
    {
        bool fullScreen = (bool)Setting<bool>(Settings.FullScreen).GetCurrentValue();
        SetResolution(width, height, GetFullScreenMode(fullScreen));
    }

    private FullScreenMode GetFullScreenMode(bool isFullScreen)
    {
        return isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    private void SetResolution(int width, int height, FullScreenMode mode)
    {
        if (width == 0)
        {
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, mode);
        }
        else
        {
            Screen.SetResolution(width, height, mode);
        }
        if (!skipConfirmingChanges)
        {
            ConfirmDisplaySettings.CheckSettingsConfirmed(() => SaveChanges(), () => RevertChanges());
        }
    }

    private void SaveChanges()
    {
        oldFullScreen = (bool)Setting<bool>(Settings.FullScreen).GetCurrentValue();
        oldResolution = (Resolution)Setting<Resolution>(Settings.Resolution).GetCurrentValue();
    }

    private void RevertChanges()
    {
        skipConfirmingChanges = true;
        ConfirmDisplaySettings.RevertToSettings(oldFullScreen, oldResolution);
        // Setting(Settings.FullScreen).SetCurrentValue(oldFullScreen);
        // Setting(Settings.Resolution).SetCurrentValue(oldResolution);
        skipConfirmingChanges = false;
    }
}
