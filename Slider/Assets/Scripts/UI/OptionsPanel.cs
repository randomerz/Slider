using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        musicSlider.onValueChanged.AddListener((float value) => { UpdateMusicVolume(); });
        sfxSlider.onValueChanged.AddListener((float value) => { UpdateSFXVolume(); });

        sfxSlider.value = AudioManager.GetSFXVolume();
        musicSlider.value = AudioManager.GetMusicVolume();
    }

    public void UpdateSFXVolume()
    {
        SettingsManager.SFXVolume = sfxSlider.value;
        AudioManager.SetSFXVolume(sfxSlider.value);
    }

    public void UpdateMusicVolume()
    {
        SettingsManager.MusicVolume = musicSlider.value;
        AudioManager.SetMusicVolume(musicSlider.value);
    }
}
