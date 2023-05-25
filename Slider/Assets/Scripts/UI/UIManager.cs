
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : Singleton<UIManager>
{
    public static System.EventHandler<System.EventArgs> OnPause;
    public static System.EventHandler<System.EventArgs> OnResume;
    public static System.EventHandler<System.EventArgs> OnCloseAllMenus;
    
    public bool isGamePaused;
    public static bool canOpenMenus = true;
    private static bool couldOpenMenusLastFrame = true; // DC: maximum jank because timing

    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject controlsPanel;
    public GameObject advOptionsPanel;
    public GameObject eventSystem;

    private void Awake()
    {
        InitializeSingleton();

        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Pause, context => _instance.OnPressPause());
        //for controller support
        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Back, context => { BackPressed(); });
    }

    private void OnDisable() {
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
        if(GameUI.instance.isMenuScene)
            return;
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

    public static void SetCanOpenMenus(bool val)
    {
        canOpenMenus = val;
    }
    

    public static bool IsUIOpen() // used for if Player can use Action
    {
        return _instance.isGamePaused;
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
        UINavigationManager.CurrentMenu = null;
       // UIManager.canOpenMenus = true;
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

    public static void InvokeCloseAllMenus(bool disableOpen = false)
    {
        canOpenMenus = !disableOpen;
        _instance.ResumeGame();

        OnCloseAllMenus.Invoke(_instance, null);
        
    }



    public void OpenPause()
    {
       // UIManager.canOpenMenus = false;
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
        else if (pausePanel.activeSelf)
        {
            CloseUI();
        }
    }

    public void LoadMainMenu()
    {
        SaveSystem.SaveGame("Quitting to Main Menu");
        SaveSystem.SetCurrentProfile(-1);
        ResumeGame();

        // Undo lazy singletons
        if(Player.GetInstance() != null)
            Player.GetInstance().ResetInventory();

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game!");
    }

}
