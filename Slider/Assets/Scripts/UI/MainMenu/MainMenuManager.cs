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

        _instance.controls.UI.Cancel.performed += context => { AudioManager.Play("UI Click"); CloseCurrentPanel(); };
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

        StartCoroutine(SelectTopmostButton());
    }

    // Select either Play or Select.
    // We need to WaitForEndOfFrame to prevent a mouse input at the splah screen from de-selecting our button.
    private IEnumerator SelectTopmostButton()
    {
        yield return new WaitForEndOfFrame();

        if (continueButton.interactable)
        {
            continueButton.Select();
        }
        else
        {
            playButton.Select();
        }
    }

    #region UI stuff

    public void CloseCurrentPanel()
    {
        if (advancedOptionsPanel.activeSelf || controlsPanel.activeSelf)
        {
            OpenOptions();
        }
        else if (newSavePanel.activeSelf)
        {
            OpenSaves();
        }
        else if (savesPanel.activeSelf || optionsPanel.activeSelf || creditsPanel.activeSelf)
        {
            CloseAllPanels();
        }
        else
        {
            QuitGame();
        }
    }

    public void CloseAllPanels()
    {
        savesPanel.SetActive(false);
        newSavePanel.SetActive(false);
        optionsPanel.SetActive(false);
        advancedOptionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        StartCoroutine(SelectTopmostButton());
    }

    public void OpenSaves()
    {
        CloseAllPanels();
        savesPanel.SetActive(true);
    }

    public void OpenNewSave(int profileIndex)
    {
        CloseAllPanels();
        newSavePanel.SetActive(true);

        newSaveProfileIndex = profileIndex;

        profileNameTextField.Select();
        profileNameTextField.ActivateInputField();
        profileNameTextField.text = "";
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
    }

    public void OpenAdvancedOptions()
    {
        CloseAllPanels();
        advancedOptionsPanel.SetActive(true);
    }

    public void OpenControls()
    {
        CloseAllPanels();
        controlsPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        CloseAllPanels();
        creditsPanel.SetActive(true);
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
