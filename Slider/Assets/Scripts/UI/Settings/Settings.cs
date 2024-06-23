using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

// Make sure to add to the end of the list because of how Unity serializes enums!
public enum Settings
{
    MasterVolume,
    SFXVolume,
    MusicVolume,
    AmbienceVolume,
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
    TargetFrameRate,
    PlayAudioWhenUnfocused,
    Locale,
    PixelFontEnabled,
}

static class SettingsMethods
{
    public static string PlayerPrefsKey(this Settings setting) => setting switch
    {
        Settings.HideCursor => "hideCursor",
        Settings.SFXVolume => "sfxVolume",
        Settings.MasterVolume => "masterVolume",
        Settings.AmbienceVolume => "ambienceVolume",
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
        Settings.PlayAudioWhenUnfocused => "playAudioWhenUnfocused",
        Settings.Locale => "locale",
        Settings.PixelFontEnabled => "pixelFontEnabled",
        _ => ""
    };
}
