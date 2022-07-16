using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

// ** THIS CLASS HAS BEEN UPDATED TO USE THE NEW SINGLETON BASE CLASS. PLEASE REPORT NEW ISSUES YOU SUSPECT ARE RELATED TO THIS CHANGE TO TRAVIS AND/OR DANIEL! **
public class UIArtifactMenus : Singleton<UIArtifactMenus>
{
    public GameObject artifactPanel;
    public UIArtifact uiArtifact;
    public ArtifactScreenAnimator screenAnimator;
    public Animator artifactAnimator;
    public UIArtifactWorldMap artifactWorldMap;

    private bool isArtifactOpen;
    private bool isClosing;

    private void Awake() 
    {
        InitializeSingleton();

        artifactWorldMap.Init();

        // check if this is pointing to the correct UIArtifact or prefab (this happens when we have scripts/prefabs extend UIArtifact)
        if (uiArtifact.gameObject.scene.name == null)
        {
            Debug.LogWarning("You might need to update my UIArtifact reference!");
        }
        DisableArtPanel();

        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Pause, context => _instance.CloseArtifact());
        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.OpenArtifact, context => _instance.OnPressArtifact());
        //Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.ArtifactRight, context => _instance.screenAnimator.NextScreen());
        //Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.ArtifactLeft, context => _instance.screenAnimator.PrevScreen());
        //Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.CycleArtifactScreens, context => _instance.screenAnimator.CycleScreen());
        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.CycleEquip, context => _instance.screenAnimator.CycleScreen());
    }

    private void OnEnable() 
    {
        PlayerInventory.OnPlayerGetCollectible += CloseArtifactListener;
        UIManager.OnCloseAllMenus += CloseArtifactListener;
    }

    private void OnDisable() 
    {
        PlayerInventory.OnPlayerGetCollectible -= CloseArtifactListener;
        UIManager.OnCloseAllMenus -= CloseArtifactListener;
    }

    public static bool IsArtifactOpen()
    {
        return _instance.isArtifactOpen;
    }

    private void OnPressArtifact()
    {
        if (isArtifactOpen)
        {
            CloseArtifact();
        }
        else if (!UIManager.IsUIOpen())
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
