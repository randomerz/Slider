using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIPause : MonoBehaviour {
    public UIManager manager;

    public void UpdateSFXVolume()  //float value
    {   
        AudioManager.SetSFXVolume(manager.sfxSlider.value);
    }

    public void UpdateMusicVolume()  //float value
    {
        AudioManager.SetMusicVolume(manager.musicSlider.value);
    }

    public void ToggleBigText(bool value)
    {
        DialogueManager.highContrastMode = value;
        DialogueManager.doubleSizeMode = value;
    }
    
    public void LoadGame()
    {
        manager.ResumeGame();
        SceneManager.LoadScene("Game");
    }

    public void LoadMainMenu()
    {
        manager.ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }

    
}