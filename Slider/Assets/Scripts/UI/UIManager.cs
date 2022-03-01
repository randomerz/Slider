
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
    public GameObject optionsPanel;
    public GameObject controlsPanel;
    public GameObject advOptionsPanel;
    public GameObject artifactPanel;
    public UIArtifact uiArtifact;
    public Animator artifactAnimator;
    public Slider sfxSlider;
    public Slider musicSlider;

    public static bool closeUI;

    private InputSettings controls;
    // private InputAction inputAction;
    // private InputActionRebindExtensions.RebindingOperation rebindingOperation;
    private void Awake()
    {
        sfxSlider.value = AudioManager.GetSFXVolume();
        musicSlider.value = AudioManager.GetMusicVolume();
        //artifactPanel.GetComponent<UIArtifact>().Awake();
        uiArtifact.Awake();
        
        controls = new InputSettings();
        // controls.m_UI_Pause =  
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

    // void StartInteractiveRebind() 
    // {   
    //     rebindingOperation = inputAction.PerformInteractiveRebinding()
    //         .WithControlsExcluding("<Mouse>/leftButton")
    //         .WithControlsExcluding("<Mouse>/rightButton")
    //         .WithControlsExcluding("<Mouse>/press")
    //         .WithControlsExcluding("<Pointer>/position")
    //         .WithCancelingThrough("<Keyboard>/escape");   
    // }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game!");
    }
    
}