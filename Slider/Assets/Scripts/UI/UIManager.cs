
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public bool isGamePaused;
    public bool isArtifactOpen;

    public GameObject pausePanel;
    public GameObject artifactPanel;
    public Slider sfxSlider;
    public Slider musicSlider;

    private void Awake()
    {
        sfxSlider.value = AudioManager.GetVolume();
        musicSlider.value = AudioManager.GetVolume();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.E))
        {
            if (isArtifactOpen)
            {
                ResumeGame();
            }
            else
            {
                OpenArtifact();
            }
        }
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        artifactPanel.SetActive(false);
        Time.timeScale = 1;
        isGamePaused = false;
        isArtifactOpen = false;
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isGamePaused = true;
    }

    public void OpenArtifact()
    {
        if (Player.IsSafe())
        {
            artifactPanel.SetActive(true);
            Time.timeScale = 0f;
            isGamePaused = true;
            isArtifactOpen = true;
        }
        else
        {
            // play error sound
        }
    }

    public void UpdateSFXVolume(float value)
    {
        AudioManager.SetVolume(value);
    }

    public void UpdateMusicVolume(float value)
    {
        AudioManager.SetVolume(value);
    }

    public void LoadGame()
    {
        ResumeGame();
        SceneManager.LoadScene("Game");
    }

    public void LoadMainMenu()
    {
        ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game!");
    }
}