
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public bool isGamePaused;
    public bool isArtifactOpen;
    public static bool canOpenMenus = true;

    public GameObject pausePanel;
    public GameObject artifactPanel;
    public Animator artifactAnimator;
    public Slider sfxSlider;
    public Slider musicSlider;

    private void Awake()
    {
        sfxSlider.value = AudioManager.GetSFXVolume();
        musicSlider.value = AudioManager.GetMusicVolume();
        artifactPanel.GetComponent<UIArtifact>().Awake();
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

        if (Input.GetKeyDown(KeyCode.Tab))
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
        Time.timeScale = 1;
        isGamePaused = false;

        if (isArtifactOpen)
        {
            Player.canMove = true;

            isArtifactOpen = false;
            artifactAnimator.SetBool("isVisible", false);
            StartCoroutine(CloseArtPanel());
        }
    }

    private IEnumerator CloseArtPanel()
    {
        yield return new WaitForSeconds(0.34f);
        artifactPanel.SetActive(false);
    }

    public void PauseGame()
    {
        if (!canOpenMenus)
            return;

        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isGamePaused = true;
    }

    public void OpenArtifact()
    {
        if (!canOpenMenus)
            return;

        if (Player.IsSafe())
        {
            artifactPanel.SetActive(true);
            isGamePaused = true;
            isArtifactOpen = true;

            Player.canMove = false;

            artifactAnimator.SetBool("isVisible", true);
        }
        else
        {
            AudioManager.Play("Artifact Error");
        }
    }

    public void UpdateSFXVolume(float value)
    {
        AudioManager.SetSFXVolume(value);
    }

    public void UpdateMusicVolume(float value)
    {
        AudioManager.SetMusicVolume(value);
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