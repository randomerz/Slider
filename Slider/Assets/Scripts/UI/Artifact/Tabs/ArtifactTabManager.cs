using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class ArtifactTabManager : MonoBehaviour 
{
    public static System.EventHandler<System.EventArgs> AfterScrollRearrage;
    public static System.EventHandler<System.EventArgs> OnUIArtifactExternallyUpdated; // To be invoked by other methods

    public List<ArtifactTab> tabs = new List<ArtifactTab>();
    protected ArtifactTab realignTab;
    protected ArtifactTab saveTab;
    protected ArtifactTab loadTab;

    private bool isRearranging;

    [Header("References")]
    public UIArtifactMenus uiArtifactMenus; // Access UIArtifact through me!

    // Tabs -- this is not a good solution but we only have one set of tabs so it's fine lol
    public Animator rearrangingTabAnimator;
    public Sprite saveTabSprite;
    public Sprite loadTabSprite;
    public Sprite saveEmptyTabSprite;
    public Sprite loadEmptyTabSprite;

    public ScrollTextEffects saveEffect;
    public ScrollTextEffects loadEffect;

    private int[,] originalGrid;

    public bool InPreview { get; private set; }

    private Coroutine hoverCoroutine = null;
    private Coroutine clickCoroutine = null;
    private bool justClickedLoad;

    protected virtual void Awake()
    {
        InitTabs();

        if (saveEffect == null || loadEffect == null)
        {
            Debug.LogError($"SaveEffect or LoadEffect was null. Probably because TabManager was overriden, please assign them in this scene!");
        }
    }

    protected void InitTabs()
    {
        realignTab = tabs[0];
        saveTab = tabs[1];
        loadTab = tabs[2];
    }

    protected virtual void OnEnable()
    {
        UIArtifact.MoveMadeOnArtifact += ButtonInteract;
        SGrid.OnSTileCollected += OnSTileCollected;
        OnUIArtifactExternallyUpdated += UIArtifactExternallyUpdated;
    }

    protected virtual void OnDisable()
    {
        UIArtifact.MoveMadeOnArtifact -= ButtonInteract;
        SGrid.OnSTileCollected -= OnSTileCollected;
        OnUIArtifactExternallyUpdated -= UIArtifactExternallyUpdated;
        justClickedLoad = false;
    }

    public virtual void SetCurrentScreen(int screenIndex)
    {
        if (realignTab == null)
            InitTabs();
            
        if (PlayerInventory.Contains("Scroll of Realigning", Area.Desert))
        {   
            // Realign case
            if (SGrid.Current.HasAllTiles()) {
                realignTab.SetIsVisible(screenIndex == realignTab.homeScreen);
                saveTab.SetIsVisible(false);
                loadTab.SetIsVisible(false);
            }
            else // Save Load Case
            {
                realignTab.SetIsVisible(false);
                saveTab.SetIsVisible(screenIndex == saveTab.homeScreen);
                loadTab.SetIsVisible(screenIndex == loadTab.homeScreen);
                SetSaveLoadTabSprites(SGrid.Current.HasRealigningGrid());
            }
        }
        else
        { 
            realignTab.SetIsVisible(false);
            saveTab.SetIsVisible(false);
            loadTab.SetIsVisible(false);
        }
    }

    private void ButtonInteract(object sender, System.EventArgs e)
    {
        justClickedLoad = false;
    }

    private void OnSTileCollected(object sender, SGrid.OnSTileEnabledArgs e)
    {
        SGrid.Current.ResetRealigningGrid();
        SetSaveLoadTabSprites(false);
    }


    #region TabSpecific
    #region Rearranging Tab
    // Rearranging tab

    public void RearrangeOnClick()
    {
        if (SGrid.Current.AllButtonsComplete())
        {
            return;
        }

        if (SGrid.Current.TargetGrid.Contains("*"))
        {
            AudioManager.Play("Artifact Error");
            return;
        }

        if (isRearranging)
        {
            return;
        }
        isRearranging = true;

        StartCoroutine(IRearrangeOnClick());
    }

    private IEnumerator IRearrangeOnClick()
    {
        PauseManager.SetPauseState(false);
        PauseManager.AddPauseRestriction(owner: gameObject);

        yield return new WaitUntil(() => UIArtifact.GetInstance().MoveQueueEmpty());

        CameraShake.ShakeIncrease(2, 1);
        AudioManager.Play("MagicChimes1");
        
        yield return new WaitForSeconds(0.5f);
        AudioManager.Play("Rumble Decrease 5s");

        UIEffects.FlashWhite(
            callbackMiddle: () => {
                SGrid.Current.RearrangeGrid();

                PauseManager.RemovePauseRestriction(owner: gameObject);

                AfterScrollRearrage?.Invoke(this, new System.EventArgs());
            }, 
            callbackEnd: () => {
                isRearranging = false;
            },
            speed: 0.5f
        );

        yield return new WaitForSeconds(1.5f);

        CameraShake.Shake(2, 1);
    }

    public void RearrangeOnHoverEnter()
    {
        rearrangingTabAnimator.SetFloat("speed", 2);
    }

    public void RearrangeOnHoverExit()
    {
        rearrangingTabAnimator.SetFloat("speed", 1);
    }

    #endregion

    #region Save and Load
    // Save tab

    public void SaveOnClick()
    {
        if (isRearranging)
            return;
        isRearranging = true;
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }
        if (clickCoroutine == null)
        {
            clickCoroutine = StartCoroutine(ISaveOnClick());  
        }
    }

    private IEnumerator ISaveOnClick()
    {
        yield return new WaitUntil(() => UIArtifact.GetInstance().MoveQueueEmpty());

        SGrid.Current.SaveRealigningGrid();

        uiArtifactMenus.uiArtifact.FlickerAllOnce();
        SetSaveLoadTabSprites(true);
        saveEffect.DoEffect();

        justClickedLoad = false;
        isRearranging = false;
        clickCoroutine = null;
    }

    // Load tab

    public void LoadOnClick()
    {
        if (SGrid.Current.realigningGrid != null)
        {
            if (isRearranging)
                return;
            isRearranging = true;
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            if (clickCoroutine == null)
            {
                clickCoroutine = StartCoroutine(ILoadOnClick());  
            }
        }
        else
        {
            AudioManager.Play("Artifact Error");
        }
    }

    private IEnumerator ILoadOnClick()
    {
        yield return new WaitUntil(() => UIArtifact.GetInstance().MoveQueueEmpty());

        bool shouldResetGrid = justClickedLoad;
        SGrid.Current.LoadRealigningGrid(shouldResetGrid);
        UpdateOriginalGrid();

        foreach (ArtifactTileButton button in uiArtifactMenus.uiArtifact.buttons)
        {
            button.SetHighlighted(false);
        }

        PlayerInventory.ReturnAnchorFromMap();

        UIEffects.FadeFromWhite();
        CameraShake.Shake(1.5f, 0.75f);
        AudioManager.Play("Slide Explosion");

        SetSaveLoadTabSprites(!shouldResetGrid);
        loadEffect.DoEffect();

        justClickedLoad = true;
        isRearranging = false;
        clickCoroutine = null;
    }

    public void LoadOnHoverEnter()
    {
        if(SGrid.Current.realigningGrid != null && clickCoroutine == null && hoverCoroutine == null)
        {
            hoverCoroutine = StartCoroutine(ILoadOnHoverEnter());
        }
    }

    private IEnumerator ILoadOnHoverEnter()
    {
        yield return new WaitUntil(() => UIArtifact.GetInstance().MoveQueueEmpty());
        InPreview = true;
        uiArtifactMenus.uiArtifact.DeselectSelectedButton();

        UpdateOriginalGrid();

        for (int x = 0; x < SGrid.Current.Width; x++)
        {
            for (int y = 0; y < SGrid.Current.Height; y++)
            {
                if(SGrid.Current.realigningGrid == null)
                {
                    Debug.LogWarning("realign grid is null. There was an issue with scroll corotutine ecxecution order");
                }
                else
                {
                    var loc = SGrid.Current.realigningGrid[x, y];
                    var button = uiArtifactMenus.uiArtifact.GetButton(SGrid.Current.realigningGrid[x, y]);
                    button.SetPosition(x, y, false);
                    button.SetHighlighted(true);
                }
            }
        }
        hoverCoroutine = null;
    }

    private void UpdateOriginalGrid()
    {
        originalGrid = new int[SGrid.Current.Width, SGrid.Current.Height];
        for (int x = 0; x < SGrid.Current.Width; x++)
        {
            for (int y = 0; y < SGrid.Current.Height; y++)
            {
                originalGrid[x, y] = UIArtifact.GetButton(x, y).islandId;
            }
        }
    }

    public void LoadOnHoverExit() => TryEndPreviewAndHover();
    
    private void UIArtifactExternallyUpdated(object sender, System.EventArgs e) => TryEndPreviewAndHover();

    private void TryEndPreviewAndHover()
    {
        if (InPreview)
        {
            InPreview = false;

            for (int x = 0; x < SGrid.Current.Width; x++)
            {
                for (int y = 0; y < SGrid.Current.Height; y++)
                {
                    int tid = originalGrid[x, y];
                    uiArtifactMenus.uiArtifact.GetButton(tid).SetPosition(x, y, false);
                    uiArtifactMenus.uiArtifact.GetButton(tid).SetHighlighted(false);
                }
            }
            originalGrid = null;
        }
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
    }

    public void SetSaveLoadTabSprites(bool b)
    {
        if (b)
        {
            saveTab.GetComponentInChildren<Image>().sprite = saveTabSprite;
            loadTab.GetComponentInChildren<Image>().sprite = loadTabSprite;
        }
        else
        {
            saveTab.GetComponentInChildren<Image>().sprite = saveEmptyTabSprite;
            loadTab.GetComponentInChildren<Image>().sprite = loadEmptyTabSprite;
        }
    }
    #endregion
    #endregion
}