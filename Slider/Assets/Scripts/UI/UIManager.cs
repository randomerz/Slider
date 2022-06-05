
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
    public static System.EventHandler<System.EventArgs> OnCloseAllMenus;
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
    public Slider sfxSlider;
    public Slider musicSlider;
    public Slider screenShakeSlider;
    public Toggle bigTextToggle;

    private void Awake()
    {
        _instance = this;

        _instance.controls = new InputSettings();
        LoadBindings();

        sfxSlider.value = AudioManager.GetSFXVolume();
        musicSlider.value = AudioManager.GetMusicVolume();

        bigTextToggle.onValueChanged.AddListener((bool value) => { UpdateBigText(); });
    }

    public static void LoadBindings()
    {
        if (_instance == null)
        {
            return;
        }

        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            _instance.controls.LoadBindingOverridesFromJson(rebinds);
        }
        _instance.controls.UI.Pause.performed += context => _instance.OnPressPause();
    }

    private void OnEnable() {
        controls.Enable();
    }

    private void OnDisable() {
        controls.Disable();

        if (!canOpenMenus)
        {
            Debug.LogWarning("UIManager was disabled without closing the menu!");
            isGamePaused = false;
            canOpenMenus = true;
        }
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
            // Debug.Log("Another menu is open, doing nothing..");
        }
        else 
        {
            PauseGame();
            OpenPause();
        }
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
        
        OnResume?.Invoke(this, null);
    }

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

    public static void InvokeCloseAllMenus()
    {
        _instance.ResumeGame();

        OnCloseAllMenus.Invoke(_instance, null);
    }



    public void OpenPause()
    {
        if (!couldOpenMenusLastFrame)
            return;

        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        advOptionsPanel.SetActive(false);

        UINavigationManager.CurrentMenu = pausePanel;
        UINavigationManager.SelectBestButtonInCurrentMenu();
    }

    public void OpenOptions()
    {
        sfxSlider.value = AudioManager.GetSFXVolume();
        musicSlider.value = AudioManager.GetMusicVolume();
        screenShakeSlider.value = SettingsManager.ScreenShake;

        if (!couldOpenMenusLastFrame)
            return;

        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
        controlsPanel.SetActive(false);
        advOptionsPanel.SetActive(false);

        UINavigationManager.CurrentMenu = optionsPanel;
        UINavigationManager.SelectBestButtonInCurrentMenu();
    }

    public void OpenControls()
    {
        if (!couldOpenMenusLastFrame)
            return;

        optionsPanel.SetActive(false);
        controlsPanel.SetActive(true);

        UINavigationManager.CurrentMenu = controlsPanel;
        UINavigationManager.SelectBestButtonInCurrentMenu();
    }
    public void OpenAdvOptions()
    {
        bigTextToggle.isOn = SettingsManager.BigTextEnabled;

        if (!couldOpenMenusLastFrame)
            return;

        optionsPanel.SetActive(false);
        advOptionsPanel.SetActive(true);

        UINavigationManager.CurrentMenu = advOptionsPanel;
        UINavigationManager.SelectBestButtonInCurrentMenu();
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

    public void UpdateScreenShake()
    {
        SettingsManager.ScreenShake = screenShakeSlider.value;
    }

    public void UpdateBigText()
    {
        // By the word of our noble lord, Boomo, long may he reign, these two lines must remain commented out
        //DialogueManager.highContrastMode = value;
        //DialogueManager.doubleSizeMode = value;

        SettingsManager.BigTextEnabled = bigTextToggle.isOn;
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
