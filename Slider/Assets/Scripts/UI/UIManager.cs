
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
    public UIArtifact uiArtifact;
    public Animator artifactAnimator;
    public Slider sfxSlider;
    public Slider musicSlider;

    public static bool closeUI;

    private InputSettings controls;

    private void Awake()
    {
        sfxSlider.value = AudioManager.GetSFXVolume();
        musicSlider.value = AudioManager.GetMusicVolume();
        //artifactPanel.GetComponent<UIArtifact>().Awake();
        uiArtifact.Awake();
        
        controls = new InputSettings();
        controls.UI.Pause.performed += context => OnPressPause();
        controls.UI.OpenArtifact.performed += context => OnPressArtifact();
    }

    private void OnEnable() {
        controls.Enable();
    }
    
    private void OnDisable() {
        controls.Disable();
    }

    void Update()
    {
        if (closeUI)
        {
            closeUI = false;
            ResumeGame();
        }

        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     if (isGamePaused)
        //     {
        //         ResumeGame();
        //     }
        //     else
        //     {
        //         PauseGame();
        //     }
        // }

        // if (Input.GetKeyDown(KeyCode.Tab))
        // {
        //     if (isArtifactOpen)
        //     {
        //         ResumeGame();
        //     }
        //     else
        //     {
        //         OpenArtifact();
        //     }
        // }
    }

    private void OnPressPause() 
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

    private void OnPressArtifact() 
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

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1;
        isGamePaused = false;

        if (isArtifactOpen)
        {
            Player.SetCanMove(true);

            isArtifactOpen = false;
            artifactAnimator.SetBool("isVisible", false);
            StartCoroutine(CloseArtPanel());
        }

        uiArtifact.DeselectCurrentButton();
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
            //UIArtifact.UpdatePushedDowns();
            isGamePaused = true;
            isArtifactOpen = true;

            Player.SetCanMove(false);

            artifactAnimator.SetBool("isVisible", true);
            uiArtifact.FlickerNewTiles();
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

    public void ToggleBigText(bool value)
    {
        DialogueManager.highContrastMode = value;
        DialogueManager.doubleSizeMode = value;
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