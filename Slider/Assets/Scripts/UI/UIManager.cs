
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static System.EventHandler<System.EventArgs> OnPause;
    public static System.EventHandler<System.EventArgs> OnResume;
    private static UIManager _instance;
    
    public bool isGamePaused;
    // public bool isArtifactOpen;
    public static bool canOpenMenus = true;
    private static bool couldOpenMenusLastFrame = true; // DC: maximum jank because timing

    private InputSettings controls;

    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject controlsPanel;
    public GameObject advOptionsPanel;
    // public GameObject artifactPanel;
    // public UIArtifact uiArtifact;
    // public Animator artifactAnimator;
    public Slider sfxSlider;
    public Slider musicSlider;

    private void Awake()
    {
        _instance = this;

        sfxSlider.value = AudioManager.GetSFXVolume();
        musicSlider.value = AudioManager.GetMusicVolume();
        

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

    private void LateUpdate() 
    {
        couldOpenMenusLastFrame = canOpenMenus;
    }


    private void OnPressPause()
    {
        if (isGamePaused && pausePanel.activeSelf)
        {
            ResumeGame();
        }
        else if (optionsPanel.activeSelf)
        {
            OpenPause();
        }
        else if (controlsPanel.activeSelf || advOptionsPanel.activeSelf)
        {
            // if in a pause sub-menu
            OpenOptions();
        }
        else if (IsUIOpen())
        {
            // if another menu is open (e.g. ocean shop)
            // do nothing
            Debug.Log("Another menu is open, doing nothing..");
        }
        else 
        {
            PauseGame();
            OpenPause();
        }
    }

    private void OnPressArtifact()
    {
        // if (isArtifactOpen)
        // {
        //     ResumeGame();
        // }
        // else
        // {
        //     OpenArtifact();
        // }
    }

    

    public static bool IsUIOpen() // used for if Player can use Action
    {
        return _instance.isGamePaused;// || _instance.isArtifactOpen;
    }

    public static void CloseUI()
    {
        _instance.ResumeGame();
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1;
        isGamePaused = false;

        // if (isArtifactOpen)
        // {
        //     Player.SetCanMove(true);
        //     isArtifactOpen = false;
        //     artifactAnimator.SetBool("isVisible", false);
        //     StartCoroutine(CloseArtPanel());
        // }

        // uiArtifact.DeselectCurrentButton();
        
        OnResume?.Invoke(this, null);
    }

    // private IEnumerator CloseArtPanel()
    // {
    //     yield return new WaitForSeconds(0.34f);
    //     artifactPanel.SetActive(false);
    // }

    // DC: this is really bad code haha
    public static void PauseGameGlobal()
    {
        _instance.PauseGame();
    }

    // DC: pauses the game, but doesn't do anything to UI
    // we should consider refactoring this to use a state machine
    public void PauseGame()
    {
        if (!couldOpenMenusLastFrame)
            return;

        Time.timeScale = 0f;
        isGamePaused = true;

        OnPause?.Invoke(this, null);
    }



    public void OpenPause()
    {
        if (!couldOpenMenusLastFrame)
            return;

        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        advOptionsPanel.SetActive(false);
    }

    public void OpenOptions()
    {
        if (!couldOpenMenusLastFrame)
            return;

        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
        controlsPanel.SetActive(false);
        advOptionsPanel.SetActive(false);
    }

    public void OpenControls()
    {
        if (!couldOpenMenusLastFrame)
            return;

        optionsPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }
    public void OpenAdvOptions()
    {
        if (!couldOpenMenusLastFrame)
            return;

        optionsPanel.SetActive(false);
        advOptionsPanel.SetActive(true);
    }

    public void BackPressed()
    {
        if (optionsPanel.activeSelf)
        {
            OpenPause();
        }
        else if (controlsPanel.activeSelf || advOptionsPanel.activeSelf)
        {
            OpenOptions();
        }
    }

    // public void OpenArtifact()
    // {
    //     if (!canOpenMenus)
    //         return;

    //     if (Player.IsSafe())
    //     {
    //         artifactPanel.SetActive(true);
    //         //UIArtifact.UpdatePushedDowns();
    //         isGamePaused = true;
    //         isArtifactOpen = true;

    //         Player.SetCanMove(false);

    //         artifactAnimator.SetBool("isVisible", true);
    //         uiArtifact.FlickerNewTiles();
    //     }
    //     else
    //     {
    //         AudioManager.Play("Artifact Error");
    //     }
    // }

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
}
