using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Obsolete]
public class OptionsPanel : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        musicSlider.onValueChanged.AddListener((float value) => { UpdateMusicVolume(); });
        sfxSlider.onValueChanged.AddListener((float value) => { UpdateSFXVolume(); });
    }

    private void OnEnable()
    {
        sfxSlider.value = AudioManager.GetSFXVolume();
        musicSlider.value = AudioManager.GetMusicVolume();
    }

    public void UpdateSFXVolume()
    {
        SettingsManager.Setting<float>(Settings.SFXVolume).CurrentValue = sfxSlider.value;
        AudioManager.SetSFXVolume(sfxSlider.value);
    }

    public void UpdateMusicVolume()
    {
        SettingsManager.Setting<float>(Settings.MusicVolume).CurrentValue = musicSlider.value;
        AudioManager.SetMusicVolume(musicSlider.value);
    }
}
