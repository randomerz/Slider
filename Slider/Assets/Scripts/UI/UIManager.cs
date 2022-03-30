
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public bool isGamePaused;
    public bool isArtifactOpen;
    public bool isQuestPanelOpen;
    public static bool canOpenMenus = true;

    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject controlsPanel;
    public GameObject advOptionsPanel;
    public GameObject artifactPanel;
    public GameObject oceanQuestPanel;
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

        _instance = this;
        _instance.controls = new InputSettings();
        LoadBindings();
    }

    public static void LoadBindings()
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            _instance.controls.LoadBindingOverridesFromJson(rebinds);
        }
        _instance.controls.UI.Pause.performed += context => _instance.OnPressPause();
        _instance.controls.UI.OpenArtifact.performed += context => _instance.OnPressArtifact();
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
            if (controlsPanel.activeSelf || advOptionsPanel.activeSelf)
            {
                OpenOptions();
            }
            else
            {
                PauseGame();
            }
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
        } else if (oceanQuestPanel.activeSelf) {
            oceanQuestPanel.SetActive(false);
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
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        advOptionsPanel.SetActive(false);
        Time.timeScale = 0f;
        isGamePaused = true;
    }

    public void OpenOptions()
    {
        if (!canOpenMenus)
            return;

        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
        controlsPanel.SetActive(false);
        advOptionsPanel.SetActive(false);
    }

    public void OpenControls()
    {
        if (!canOpenMenus)
            return;

        optionsPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }
    public void OpenAdvOptions()
    {
        if (!canOpenMenus)
            return;

        optionsPanel.SetActive(false);
        advOptionsPanel.SetActive(true);
    }

    public void BackPressed()
    {
        if (optionsPanel.activeSelf)
        {
            PauseGame();
        }
        else if (controlsPanel.activeSelf || advOptionsPanel.activeSelf)
        {
            OpenOptions();
        }
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

    public void UpdateSFXVolume()  //float value
    {
        AudioManager.SetSFXVolume(sfxSlider.value);
    }

    public void UpdateMusicVolume()  //float value
    {
        AudioManager.SetMusicVolume(musicSlider.value);
    }

    // public void ToggleBigText(bool value)
    // {
    //     DialogueManager.highContrastMode = value;
    //     DialogueManager.doubleSizeMode = value;
    // }

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

    public void OpenShopPanel() {
          if (!canOpenMenus)
              return;
          if (Player.IsSafe())
          {
              oceanQuestPanel.SetActive(true);
              //UIArtifact.UpdatePushedDowns();
              isGamePaused = true;
              isQuestPanelOpen = true;
              Time.timeScale = 0;
              Player.SetCanMove(false);
              artifactAnimator.SetBool("isVisible", true); //i'll keep this in rn
              uiArtifact.FlickerNewTiles();
          }
          else
          {
              AudioManager.Play("OceanQuestPanel Error");
          }
      }

}
