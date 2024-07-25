using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    private bool keepArtifactOpen = false;

    private List<Button> selectibles;
    private Dictionary<Button, Navigation.Mode> navMode;


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
        SaveSelectibles();

        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Pause, context => _instance.CloseArtifact());
        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.OpenArtifact, context => _instance.OnPressArtifact());
        //Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.ArtifactRight, context => _instance.screenAnimator.NextScreen());
        //Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.ArtifactLeft, context => _instance.screenAnimator.PrevScreen());
        //Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.CycleArtifactScreens, context => _instance.screenAnimator.CycleScreen());
        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.CycleEquip, context => _instance.screenAnimator.CycleScreen()); // CONTROLER BIND ONLY
        //For controller support
        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Back, context => _instance.CloseArtifact());

    }

    private void Start()
    {
        ToggleNavigation(Controls.CurrentControlScheme);
    }

    private void ToggleNavigation(string s)
    {
        if (s == Controls.CONTROL_SCHEME_CONTROLLER)
        {
            EnableNavigation();
        } 
        else
        {
            DisableNavigation();
        }
    }

    private void SaveSelectibles()
    {
        navMode = new();
        selectibles = GetComponentsInChildren<Button>(true).ToList();
        foreach(Button s in selectibles)
        {
            navMode.Add(s, s.navigation.mode);
        }
        ToggleNavigation(Controls.CurrentControlScheme);
    }


    private void DisableNavigation()
    {
        foreach(Selectable s in selectibles)
        {
            if(s.gameObject == null) continue;
            var nav = s.navigation;
            nav.mode = Navigation.Mode.None;
            s.navigation = nav;
        }
    }

    private void EnableNavigation()
    {
        foreach(Button s in selectibles)
        {
            if(s.gameObject == null) continue;
            var nav = s.navigation;
            nav.mode = navMode[s];
            s.navigation = nav;
        }
    }

    private void OnEnable() 
    {
        PlayerInventory.OnPlayerGetCollectible += CloseArtifactListener;
        ItemPickupEffect.OnCutsceneStart += CloseArtifactListener;
        //UIManager.OnCloseAllMenus += CloseArtifactListenerNoOpen;
        PauseManager.PauseStateChanged += OnPauseStateChanged;
        FactoryTimeGlitch.TimeGlitchPauseStateChanged += OnPauseStateChanged;
        Player.OnControlSchemeChanged += ToggleNavigation;
    }

    private void OnPauseStateChanged(bool newPausedState)
    {
        if (newPausedState)
        {
            CloseArtifact(false);
        }
    }

    private void OnDisable() 
    {
        PlayerInventory.OnPlayerGetCollectible -= CloseArtifactListener;
        ItemPickupEffect.OnCutsceneStart -= CloseArtifactListener;
        //UIManager.OnCloseAllMenus -= CloseArtifactListenerNoOpen;
        PauseManager.PauseStateChanged -= OnPauseStateChanged;
        FactoryTimeGlitch.TimeGlitchPauseStateChanged -= OnPauseStateChanged;
        Player.OnControlSchemeChanged -= ToggleNavigation;
    }

    public static bool IsArtifactOpen()
    {
        return _instance != null && _instance.isArtifactOpen;
    }

    public void IsArtifactOpen(Condition c) => c.SetSpec(IsArtifactOpen());

    public static void TurnInArtifact()
    {
        _instance.hasArtifact = false;
    }

    private void OnPressArtifact()
    {
        if (isArtifactOpen)
        {
            CloseArtifact();
        }
        else if (!PauseManager.IsPaused)
        {
            OpenArtifact();
        }
    }

    public void OpenArtifact()
    {
        if (!PauseManager.CanPause() || isClosing || !hasArtifact)
        {
            return;
        }

        artifactPanel.SetActive(true);
        isArtifactOpen = true;

        PauseManager.AddPauseRestriction(owner: gameObject);
        Player.SetCanMove(false);

        artifactAnimator.SetBool("isVisible", true);
        uiArtifact.FlickerNewTiles();

        OnArtifactOpened?.Invoke(this, null);
    }

    public void CloseArtifact(bool canOpen = true)
    {
        if (keepArtifactOpen)
        {
            AudioManager.PickSound("Hat Click").WithVolume(0.5f).WithPitch(0.7f).AndPlay();
            return;
        }

        if (isArtifactOpen)
        {
            isArtifactOpen = false;
            uiArtifact.DeselectSelectedButton();

            Player.SetCanMove(true);
            PauseManager.RemovePauseRestriction(owner: gameObject);

            artifactAnimator.SetBool("isVisible", false);
            isClosing = true;
        }
    }

    public static void SetKeepArtifactOpen(bool value)
    {
        _instance.keepArtifactOpen = value;
        if (!IsArtifactOpen())
        {
            _instance.OpenArtifact();
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
