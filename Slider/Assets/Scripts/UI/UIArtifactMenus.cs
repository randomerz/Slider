using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

// ** THIS CLASS HAS BEEN UPDATED TO USE THE NEW SINGLETON BASE CLASS. PLEASE REPORT NEW ISSUES YOU SUSPECT ARE RELATED TO THIS CHANGE TO TRAVIS AND/OR DANIEL! **
public class UIArtifactMenus : Singleton<UIArtifactMenus>
{
    public static System.EventHandler<System.EventArgs> OnArtifactOpened;

    public GameObject artifactPanel;
    public UIArtifact uiArtifact;
    public ArtifactScreenAnimator screenAnimator;
    public Animator artifactAnimator;
    public UIArtifactWorldMap artifactWorldMap;

    public bool hasArtifact = true;
    private bool isArtifactOpen;
    private bool isClosing = false;

    private void Awake() 
    {
        InitializeSingleton();

        artifactWorldMap.Init(); // TODO: remove this! make it standalone 

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
        ItemPickupEffect.OnCutsceneStart += CloseArtifactListener;
        UIManager.OnCloseAllMenus += CloseArtifactListenerNoOpen;
    }

    private void OnDisable() 
    {
        PlayerInventory.OnPlayerGetCollectible -= CloseArtifactListener;
        ItemPickupEffect.OnCutsceneStart -= CloseArtifactListener;
        UIManager.OnCloseAllMenus -= CloseArtifactListenerNoOpen;
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
        if (!UIManager.canOpenMenus || isClosing || !hasArtifact)
        {
            // Debug.LogWarning("UIManager: " + UIManager.canOpenMenus);
            // Debug.LogWarning("isClosing: " + isClosing);
            // Debug.LogWarning("hasArtifact: " + hasArtifact);
            return;
        }

        artifactPanel.SetActive(true);
        isArtifactOpen = true;

        UIManager.PauseGameGlobal();
        UIManager.canOpenMenus = false;
        
        // scuffed parts
        Player.SetCanMove(false);
        Time.timeScale = 1;

        artifactAnimator.SetBool("isVisible", true);
        uiArtifact.FlickerNewTiles();

        OnArtifactOpened?.Invoke(this, null);
    }

    public void CloseArtifact(bool canOpen = true)
    {
        if (isArtifactOpen)
        {
            isArtifactOpen = false;
            uiArtifact.DeselectSelectedButton();
            Player.SetCanMove(true);

            UIManager.CloseUI();
            UIManager.canOpenMenus = canOpen;

            artifactAnimator.SetBool("isVisible", false);
            isClosing = true;
        }
    }

    private void CloseArtifactListener(object sender, System.EventArgs e)
    {
        CloseArtifact();
    }

    private void CloseArtifactListenerNoOpen(object sender, System.EventArgs e)
    {
        CloseArtifact(false);
    }

    public void DisableArtPanel()
    {
        artifactPanel.SetActive(false);
        isClosing = false;
    }


    public void OpenArtifactAndShow(int screenIndex, bool justCollectedItem=false)
    {
        StartCoroutine(IOpenArtifactAndShow(screenIndex, justCollectedItem));
    }

    private IEnumerator IOpenArtifactAndShow(int screenIndex, bool justCollectedItem)
    {
        if (justCollectedItem)
            yield return new WaitForSeconds(2.75f); // magic number if just picked an item

        OpenArtifact();

        if (screenIndex != 0)
        {
            yield return new WaitForSeconds(0.3f); // magic number

            screenAnimator.SetScreen(screenIndex);
            
            // For the gif of the week!
            // gifAnimation.ClearAllAreas();
            // yield return new WaitForSeconds(0.6f);
            // StartCoroutine(gifAnimation.AnimateAllAreas());
        }
    }
}
