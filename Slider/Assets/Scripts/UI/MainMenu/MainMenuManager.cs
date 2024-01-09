using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;

public class MainMenuManager : Singleton<MainMenuManager>
{
    
    private int continueProfileIndex = -1;

    [Header("Animators")]
    public Animator titleAnimator;
    public Animator textAnimator;
    public Animator playerAnimator;
    public Animator mainMenuButtonsAnimator;
    public Animator mainMenuQuitButtonAnimator;
    public Animator mainMenuBackgroundAnimator;

    public GameObject mainMenuPanel;

    [Header("Other References")]
    public Button continueButton;
    public TextMeshProUGUI continueText;
    public Button playButton;
    public MainMenuWashingMachineManager washingMachineManager;

    public MainMenuSaveButton[] saveProfileButtons;

    private System.IDisposable listener;

    public static bool KeyboardEnabled { get; set; }
    
    private void Awake() {
        InitializeSingleton(this);
    }

    void Start()
    {
        StartCoroutine(OpenCutscene());

        CheckContinueButton();

        AudioManager.PlayMusic("Main Menu");
        AudioManager.SetGlobalParameter("MainMenuActivated", 0);

        // any key listener moved to OpenCutscene()
    }

    public static MainMenuManager GetInstance() 
    {
        return _instance;
    }

    private void OnDisable() 
    {
        listener?.Dispose();
    }

    private void Update() 
    {
        CheckContinueButton();
    }

    private void OnAnyButtonPress() 
    {
        listener.Dispose();
        StartMainMenu();
    }

    private bool AreAnyProfilesLoaded()
    {
        return SaveSystem.GetProfile(0) != null || SaveSystem.GetProfile(1) != null || SaveSystem.GetProfile(2) != null;
    }

    private bool CheckContinueButton()
    {
        if (!AreAnyProfilesLoaded())
        {
            continueProfileIndex = -1;
            continueButton.interactable = false;
            continueText.color = GameSettings.lightGray;
            return false;
        }
        else
        {
            continueProfileIndex = SaveSystem.GetRecentlyPlayedIndex();
            continueButton.interactable = true;
            continueText.color = GameSettings.white;
            return true;
        }
    }

    public void ContinueGame()
    {
        SaveSystem.LoadSaveProfile(continueProfileIndex);
    }

    private IEnumerator OpenCutscene()
    {
        yield return null;

        listener = InputSystem.onAnyButtonPress.Call(ctrl => OnAnyButtonPress()); // this is really janky, we may want to switch to "press start"
        UIEffects.ClearScreen(); // From EndOfGameManager.GoToMainMenu()

        yield return new WaitForSeconds(1f);
        
        CameraShake.ShakeIncrease(2.1f, 0.1f);

        yield return new WaitForSeconds(2f);

        CameraShake.Shake(0.25f, 0.2f);

        yield return new WaitForSeconds(1f);

        textAnimator.SetBool("isVisible", true);
    }

    private void StartMainMenu()
    {
        StopAllCoroutines();
        CameraShake.StopShake();

        titleAnimator.SetBool("isUp", true);
        playerAnimator.SetBool("isUp", true);
        mainMenuButtonsAnimator.SetBool("isUp", true);
        mainMenuQuitButtonAnimator.SetBool("isVisible", true);
        mainMenuBackgroundAnimator.SetBool("isVisible", true);
        textAnimator.SetBool("isVisible", false);

        AudioManager.SetGlobalParameter("MainMenuActivated", 1);

        washingMachineManager.Activate();
        
        UINavigationManager.CurrentMenu = mainMenuPanel;
        UINavigationManager.LockoutSelectablesInCurrentMenu(SelectTopmostButton, 1);
    }


    private void SelectTopmostButton()
    {
        StartCoroutine(ISelectTopmostButton());
    }
    private IEnumerator ISelectTopmostButton()
    {
        // Safety to prevent inputs from triggering a button immediately after opening the menu
        yield return new WaitForEndOfFrame();
        UINavigationManager.SelectBestButtonInCurrentMenu();
    }
    
    public static void QuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit(0);
    }
}
