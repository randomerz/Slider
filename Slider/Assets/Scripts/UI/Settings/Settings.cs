using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Settings
{
    SFXVolume,
    MusicVolume,
    ScreenShake,
    BigTextEnabled,
    HighContrastTextEnabled,
    HideCursor,
    MiniPlayerIcon,
    AutoMove,
    Colorblind,
    DevConsole,
    ScreenMode,
    Resolution,
    Vsync,
    TargetFrameRate
}

static class SettingsMethods
{
    public static string PlayerPrefsKey(this Settings setting) => setting switch
    {
        Settings.HideCursor => "hideCursor",
        Settings.SFXVolume => "sfxVolume",
        Settings.MusicVolume => "musicVolume",
        Settings.ScreenShake => "screenShake",
        Settings.BigTextEnabled => "bigTextEnabled",
        Settings.HighContrastTextEnabled => "highContrastTextEnabled",
        Settings.MiniPlayerIcon => "miniPlayerIcon",
        Settings.AutoMove => "autoMove",
        Settings.Colorblind => "colorblind",
        Settings.DevConsole => "devConsole",
        Settings.ScreenMode => "screenMode",
        Settings.Resolution => "resolution",
        Settings.Vsync => "vsync",
        Settings.TargetFrameRate => "targetFrameRate",
        _ => ""
    };
}
