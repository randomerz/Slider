using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class UIArtifactMenus : MonoBehaviour
{
    public static UIArtifactMenus _instance;

    public GameObject artifactPanel;
    public UIArtifact uiArtifact;
    public ArtifactScreenAnimator screenAnimator;
    public Animator artifactAnimator;
    public UIArtifactWorldMap artifactWorldMap;

    private bool isArtifactOpen;
    private bool isClosing;
    private InputSettings controls;

    private void Awake() 
    {
        _instance = this;

        artifactWorldMap.Init();

        // check if this is pointing to the correct UIArtifact or prefab (this happens when we have scripts/prefabs extend UIArtifact)
        if (uiArtifact.gameObject.scene.name == null)
        {
            Debug.LogWarning("You might need to update my UIArtifact reference!");
        }
        
        _instance.controls = new InputSettings();
        LoadBindings();
    }

    private void OnEnable() 
    {
        controls.Enable();

        PlayerInventory.OnPlayerGetCollectible += CloseArtifactListener;
        UIManager.OnCloseAllMenus += CloseArtifactListener;
    }

    private void OnDisable() 
    {
        controls.Disable();    

        PlayerInventory.OnPlayerGetCollectible -= CloseArtifactListener;
        UIManager.OnCloseAllMenus -= CloseArtifactListener;
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
        
        _instance.controls.UI.Pause.performed += context => _instance.CloseArtifact();
        _instance.controls.UI.OpenArtifact.performed += context => _instance.OnPressArtifact();
        
        // Disabled for now, we should add key rebinding for this and maybe make it like Next Panel Left and Next Panel Right
        //_instance.controls.UI.ArtifactRight.performed += context => _instance.screenAnimator.NextScreen();
        //_instance.controls.UI.ArtifactLeft.performed += context => _instance.screenAnimator.PrevScreen();
    }


    private void OnPressArtifact()
    {
        if (isArtifactOpen)
        {
            CloseArtifact();
        }
        else
        {
            OpenArtifact();
        }
    }

    public void OpenArtifact()
    {
        if (!UIManager.canOpenMenus || isClosing)
            return;

        if (Player.IsSafe()) // always true for now
        {
            artifactPanel.SetActive(true);
            isArtifactOpen = true;

            UIManager.PauseGameGlobal();
            UIManager.canOpenMenus = false;
            
            // scuffed parts
            Player.SetCanMove(false);
            Time.timeScale = 1;

            artifactAnimator.SetBool("isVisible", true);
            uiArtifact.FlickerNewTiles();
        }
        else
        {
            AudioManager.Play("Artifact Error");
        }
    }

    public void CloseArtifact()
    {
        if (isArtifactOpen)
        {
            isArtifactOpen = false;
            uiArtifact.DeselectCurrentButton();
            Player.SetCanMove(true);

            UIManager.CloseUI();
            UIManager.canOpenMenus = true;
            
            artifactAnimator.SetBool("isVisible", false);
            isClosing = true;
        }
    }

    private void CloseArtifactListener(object sender, System.EventArgs e)
    {
        CloseArtifact();
    }

    public void DisableArtPanel()
    {
        artifactPanel.SetActive(false);
        isClosing = false;
    }
}
