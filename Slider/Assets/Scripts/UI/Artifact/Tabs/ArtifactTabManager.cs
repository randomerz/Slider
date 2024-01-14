using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ArtifactTabManager : MonoBehaviour 
{
    public static System.EventHandler<System.EventArgs> AfterScrollRearrage;

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

    private int[,] originalGrid;

    private bool inPreview;

    protected virtual void Awake()
    {
        InitTabs();
    }

    protected void InitTabs()
    {
        realignTab = tabs[0];
        saveTab = tabs[1];
        loadTab = tabs[2];
    }

    public virtual void SetCurrentScreen(int screenIndex)
    {
        if(realignTab == null)
            InitTabs();
        #region SaveLoadRealign Cases
        if (PlayerInventory.Contains("Scroll of Realigning", Area.Desert))
        {   
            //Realign case
            if (SGrid.Current.HasAllTiles() && !SGrid.Current.AllButtonsComplete()) {
                realignTab.SetIsVisible(screenIndex == realignTab.homeScreen);
                saveTab.SetIsVisible(false);
                loadTab.SetIsVisible(false);
            }
            else //Save Load Case
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
        #endregion
    }


    #region TabSpecific
    #region Rearranging Tab
    // Rearranging tab

    public void RearrangeOnClick()
    {
        if (isRearranging)
            return;
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

        UIEffects.FlashWhite(callbackMiddle: () => {
            SGrid.Current.RearrangeGrid();

            PauseManager.RemovePauseRestriction(owner: gameObject);
            isRearranging = false;

            AfterScrollRearrage?.Invoke(this, new System.EventArgs());
        }, speed: 0.5f);

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
        StartCoroutine(ISaveOnClick());
    }

    private IEnumerator ISaveOnClick()
    {
        yield return new WaitUntil(() => UIArtifact.GetInstance().MoveQueueEmpty());
        SGrid.Current.SaveRealigningGrid();
        uiArtifactMenus.uiArtifact.FlickerAllOnce();
        SetSaveLoadTabSprites(true);
        isRearranging = false;
    }

    // Load tab

    public void LoadOnClick()
    {
        if (SGrid.Current.realigningGrid != null)
        {
            if (isRearranging)
                return;
            isRearranging = true;
            StartCoroutine(ILoadOnClick());  
        }
        else
        {
            AudioManager.Play("Artifact Error");
        }
    }

    private IEnumerator ILoadOnClick()
    {
        yield return new WaitUntil(() => UIArtifact.GetInstance().MoveQueueEmpty());

        SGrid.Current.LoadRealigningGrid();
        foreach (ArtifactTileButton button in uiArtifactMenus.uiArtifact.buttons)
        {
            button.SetHighlighted(false);
        }

        PlayerInventory.ReturnAnchorFromMap();

        UIEffects.FadeFromWhite();
        CameraShake.Shake(1.5f, 0.75f);
        AudioManager.Play("Slide Explosion");

        SetSaveLoadTabSprites(false);
        isRearranging = false;
    }

    public void LoadOnHoverEnter()
    {
        if(SGrid.Current.realigningGrid != null)
        {
            StartCoroutine(ILoadOnHoverEnter());
        }
    }

    private IEnumerator ILoadOnHoverEnter()
    {
        yield return new WaitUntil(() => UIArtifact.GetInstance().MoveQueueEmpty());
        inPreview = true;
        uiArtifactMenus.uiArtifact.DeselectSelectedButton();
        originalGrid = new int[SGrid.Current.Width, SGrid.Current.Height];
        for (int x = 0; x < SGrid.Current.Width; x++)
        {
            for (int y = 0; y < SGrid.Current.Height; y++)
            {
                int tid = SGrid.Current.GetGrid()[x, y].islandId;
                originalGrid[x, y] = tid;
                uiArtifactMenus.uiArtifact.GetButton(SGrid.Current.realigningGrid[x, y]).SetPosition(x, y, false);
                uiArtifactMenus.uiArtifact.GetButton(SGrid.Current.realigningGrid[x, y]).SetHighlighted(true);
            }
        }
    }

    public void LoadOnHoverExit()
    {
        if (SGrid.Current.realigningGrid != null && inPreview)
        {
            for (int x = 0; x < SGrid.Current.Width; x++)
            {
                for (int y = 0; y < SGrid.Current.Height; y++)
                {
                    int tid = originalGrid[x, y];
                    uiArtifactMenus.uiArtifact.GetButton(tid).SetPosition(x, y, false);
                    uiArtifactMenus.uiArtifact.GetButton(tid).SetHighlighted(false);
                }
            }
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