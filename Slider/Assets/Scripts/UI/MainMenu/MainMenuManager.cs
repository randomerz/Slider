using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;

// TODO: 
//  - refactor options into an options panel -- for now the options buttons are dead
//  - fix Continue button (see in Update())

public class MainMenuManager : MonoBehaviour
{
    public string cutsceneSceneName;

    private int newSaveProfileIndex = -1;

    [Header("Animators")]
    public Animator titleAnimator;
    public Animator textAnimator;
    public Animator playerAnimator;
    public Animator mainMenuButtonsAnimator;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject savesPanel;
    public GameObject newSavePanel;
    public TMP_InputField profileNameTextField;
    public GameObject optionsPanel;
    public GameObject advancedOptionsPanel;
    public GameObject controlsPanel;
    public GameObject creditsPanel;

    [Header("Other References")]
    public Button continueButton;
    public TextMeshProUGUI continueText;
    public Button playButton;

    private System.IDisposable listener;
    private InputSettings controls;

    private static MainMenuManager _instance;
    
    private void Awake() {
        _instance = this;
        
        _instance.controls = new InputSettings();
        LoadBindings();
    }

    void Start()
    {
        StartCoroutine(OpenCutscene());

        listener = InputSystem.onAnyButtonPress.Call(ctrl => OnAnyButtonPress()); // this is really janky, we may want to switch to "press start"

        _instance.controls.UI.Back.performed += context => { AudioManager.Play("UI Click"); CloseCurrentPanel(); };

        // Pressing a navigation key selects a button is one is not already selected
        _instance.controls.UI.Navigate.performed += context => 
        {
            if (!UINavigationManager.ButtonInCurrentMenuIsSelected())
            {
                UINavigationManager.SelectBestButtonInCurrentMenu();
            }
        };
    }

    private void OnEnable() {
        controls.Enable();
    }

    private void OnDisable() {
        controls.Disable();
        listener?.Dispose();
    }

    private void Update() {
        // todo: fix this
        // continueButton.interactable = SaveSystem.Current != null;
        // continueText.color = SaveSystem.Current != null ? GameSettings.white : GameSettings.darkGray;
    }

    public static void LoadBindings()
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            _instance.controls.LoadBindingOverridesFromJson(rebinds);
        }
        
        _instance.controls.UI.Pause.performed += context => _instance.CloseCurrentPanel();
    }


    private void OnAnyButtonPress() 
    {
        listener.Dispose();
        StartMainMenu();

        if (!AreAnyProfilesLoaded())
        {
            OpenNewSave(0);
        }


    }

    private bool AreAnyProfilesLoaded()
    {
        return SaveSystem.GetProfile(0) != null || SaveSystem.GetProfile(1) != null || SaveSystem.GetProfile(2) != null;
    }

    private IEnumerator OpenCutscene()
    {
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
        textAnimator.SetBool("isVisible", false);

        StartCoroutine(IStartMainMenu());
    }

    // We need this to provide a delay before we accept keyboard inputs when moving from splash screen
    // to buttons screen. Otherwise spamming buttons can lead to immediately entering the play menu.
    private IEnumerator IStartMainMenu()
    {
        UINavigationManager.CurrentMenu = mainMenuPanel;
        yield return new WaitForSeconds(1);
        StartCoroutine(SelectTopmostButton());
    }

    private IEnumerator SelectTopmostButton()
    {
        // Safety to prevent inputs from triggering a button immediately after opening the menu
        yield return new WaitForEndOfFrame();
        UINavigationManager.SelectBestButtonInCurrentMenu();
    }

    #region UI stuff

    public void CloseCurrentPanel()
    {
        if (advancedOptionsPanel.activeSelf || controlsPanel.activeSelf)
        {
            OpenOptions();
            UINavigationManager.CurrentMenu = optionsPanel;
        }
        else if (newSavePanel.activeSelf)
        {
            OpenSaves();
            UINavigationManager.CurrentMenu = savesPanel;
        }
        else if (savesPanel.activeSelf || optionsPanel.activeSelf || creditsPanel.activeSelf)
        {
            CloseAllPanels();
            UINavigationManager.CurrentMenu = mainMenuPanel;
        }
        else
        {
            QuitGame();
        }

        StartCoroutine(SelectTopmostButton());
    }

    public void CloseAllPanels()
    {
        savesPanel.SetActive(false);
        newSavePanel.SetActive(false);
        optionsPanel.SetActive(false);
        advancedOptionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        UINavigationManager.CurrentMenu = mainMenuPanel;
        StartCoroutine(SelectTopmostButton());
    }

    public void OpenSaves()
    {
        CloseAllPanels();
        savesPanel.SetActive(true);
        UINavigationManager.CurrentMenu = savesPanel;
    }

    public void OpenNewSave(int profileIndex)
    {
        CloseAllPanels();
        newSavePanel.SetActive(true);

        newSaveProfileIndex = profileIndex;

        profileNameTextField.Select();
        profileNameTextField.ActivateInputField();
        profileNameTextField.text = "";
        UINavigationManager.CurrentMenu = newSavePanel;
    }

    public void OnTextFieldChangeText(string text)
    {
        if (text.Contains("\n"))
        {
            profileNameTextField.text = text.Replace("\n", "");
            StartNewGame();
        }
    }

    public void OpenOptions()
    {
        CloseAllPanels();
        optionsPanel.SetActive(true);
        UINavigationManager.CurrentMenu = optionsPanel;
        StartCoroutine(SelectTopmostButton());
    }

    public void OpenAdvancedOptions()
    {
        CloseAllPanels();
        advancedOptionsPanel.SetActive(true);
        UINavigationManager.CurrentMenu = advancedOptionsPanel;
        StartCoroutine(SelectTopmostButton());
    }

    public void OpenControls()
    {
        CloseAllPanels();
        controlsPanel.SetActive(true);
        UINavigationManager.CurrentMenu = controlsPanel;
        StartCoroutine(SelectTopmostButton());
    }

    public void OpenCredits()
    {
        CloseAllPanels();
        creditsPanel.SetActive(true);
        UINavigationManager.CurrentMenu = creditsPanel;
        StartCoroutine(SelectTopmostButton());
    }

    #endregion
    
    public void QuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit(0);
    }


    public void StartNewGame()
    {
        string profileName = profileNameTextField.text;

        Debug.Log("Starting new game with profile: " + profileName);

        if (profileName.Length == 0)
            return;

        SaveSystem.SetProfile(newSaveProfileIndex, new SaveProfile(profileName));
        SaveSystem.SetCurrentProfile(newSaveProfileIndex);

        LoadCutscene();
    }

    public void LoadCutscene()
    {
        // SceneManager.LoadSceneAsync(cutsceneSceneName, LoadSceneMode.Additive);
        UIEffects.FadeToBlack(() => {SceneManager.LoadScene(cutsceneSceneName);});
    }

    public void StartGameWithCurrentSave()
    {
        if (SaveSystem.Current == null)
        {
            Debug.LogError("Tried to continue game, but Current save was null!");
            return;
        }

        // load last scene
        Debug.Log("Continuing from last scene of profile " + SaveSystem.Current.GetProfileName());

        Debug.LogWarning("lol just kidding loading village");
        SceneManager.LoadScene("Village");
    }
}
