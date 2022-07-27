using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ArtifactTabManager : MonoBehaviour 
{
    public List<ArtifactTab> tabs = new List<ArtifactTab>();
    private ArtifactTab fragRealignTab;
    private ArtifactTab realignTab;
    private ArtifactTab saveTab;
    private ArtifactTab loadTab;
    private ArtifactTab previewTab;

    private bool isRearranging;



    [Header("References")]
    public UIArtifactMenus uiArtifactMenus; // Access UIArtifact through me!

    // Tabs -- this is not a good solution but we only have one set of tabs so it's fine lol
    public Animator rearrangingTabAnimator;
    public Animator rearrangingFragTabAnimator;
    public Animator previewTabAnimator;
    public Sprite saveTabSprite;
    public Sprite loadTabSprite;
    public Sprite saveEmptyTabSprite;
    public Sprite loadEmptyTabSprite;

    private int[,] originalGrid;
    private ArtifactTileButton empty;
    private ArtifactTileButton middle;

    private void Awake()
    {
        realignTab = tabs[0];
        saveTab = tabs[1];
        loadTab = tabs[2];
        fragRealignTab = tabs[3];
        previewTab = tabs[4];
    }
    public void SetCurrentScreen(int screenIndex)
    {
        if (PlayerInventory.Contains("Scroll of Realigning", Area.Desert)
            && SGrid.Current.GetActiveTiles().Count == SGrid.Current.GetTotalNumTiles()
            && SGrid.GetNumButtonCompletions() != SGrid.Current.GetTotalNumTiles())
        {
            realignTab.SetIsVisible(screenIndex == realignTab.homeScreen);
            saveTab.SetIsVisible(false);
            loadTab.SetIsVisible(false);
        }
        else if (PlayerInventory.Contains("Scroll of Realigning", Area.Desert))
        {
            saveTab.SetIsVisible(screenIndex == saveTab.homeScreen);
            loadTab.SetIsVisible(screenIndex == loadTab.homeScreen);
            SetSaveLoadTabSprites(SGrid.Current.HasRealigningGrid());
            if (SGrid.Current.GetArea() == Area.MagiTech)
            {
                //This enables the preview tab!
                previewTab.SetIsVisible(true);
                MagiTechArtifact artifact = (MagiTechArtifact)uiArtifactMenus.uiArtifact;
                int direction = artifact.PlayerIsInPast ? -1 : 1;
                previewTabAnimator.SetFloat("speed", direction);
            }
            fragRealignTab.SetIsVisible(false);
        }
        else if (SGrid.Current.GetArea() == Area.Desert
                 && PlayerInventory.Contains("Scroll Frag", Area.Desert)
                 && !PlayerInventory.Contains("Scroll of Realigning", Area.Desert))
        {
            fragRealignTab.SetIsVisible(screenIndex == fragRealignTab.homeScreen);
        }
        else
        {
            realignTab.SetIsVisible(false);
            fragRealignTab.SetIsVisible(false);
            saveTab.SetIsVisible(false);
            loadTab.SetIsVisible(false);
            previewTab.SetIsVisible(false);
        }
    }


    #region TabSpecific

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
        UIManager.InvokeCloseAllMenus();
        UIManager.canOpenMenus = false;

        CameraShake.ShakeIncrease(2, 1);
        AudioManager.Play("Slide Explosion"); // TODO: fix sfx
        
        yield return new WaitForSeconds(0.5f);
        AudioManager.Play("Slide Explosion");

        UIEffects.FlashWhite(callbackMiddle: () => {
            // Do the rearranging!
            //Debug.Log("Rearranged!");
            SGrid.Current.RearrangeGrid();


            UIManager.canOpenMenus = true;
            isRearranging = false;
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


    #region Save and Load
    // Save tab

    public void SaveOnClick()
    {
        SGrid.Current.SaveRealigningGrid();
        uiArtifactMenus.uiArtifact.FlickerAllOnce();
        SetSaveLoadTabSprites(true);
    }

    // Load tab

    public void LoadOnClick()
    {
        if (SGrid.Current.realigningGrid != null)
        {
            // Do the rearranging!
            SGrid.Current.LoadRealigningGrid();
            foreach (ArtifactTileButton button in uiArtifactMenus.uiArtifact.buttons)
            {
                button.SetHighlighted(false);
            }
            //Debug.Log("Loaded!");


            UIEffects.FadeFromWhite();
            CameraShake.Shake(1.5f, 0.75f);
            AudioManager.Play("Slide Explosion");

            SetSaveLoadTabSprites(false);
        }
        else
        {
            AudioManager.Play("Artifact Error");
        }
        // StartCoroutine(ILoadOnClick());
    }

    private IEnumerator ILoadOnClick()
    {
        yield return null;
    }

    public void LoadOnHoverEnter()
    {
        //get the realignGrid, put the button stuff in the order based on that, then call bggrid set
        if(SGrid.Current.realigningGrid != null)
        {
            Debug.Log("Previewed!");
            uiArtifactMenus.uiArtifact.DeselectSelectedButton();
            originalGrid = new int[SGrid.Current.Width, SGrid.Current.Height];
            for (int x = 0; x < SGrid.Current.Width; x++)
            {
                for (int y = 0; y < SGrid.Current.Height; y++)
                {
                    //Debug.Log(SGrid.current.saveGrid[x, y] + " button array index: " + (SGrid.current.saveGrid[x, y] - 1) + " " + x + " " + y);
                    int tid = SGrid.Current.GetGrid()[x, y].islandId;
                    originalGrid[x, y] = tid;
                    uiArtifactMenus.uiArtifact.GetButton(SGrid.Current.realigningGrid[x, y]).SetPosition(x, y);
                    uiArtifactMenus.uiArtifact.GetButton(SGrid.Current.realigningGrid[x, y]).SetHighlighted(true);
                }
            }
        }
    }

    public void LoadOnHoverExit()
    {
        //reset
        //get the id's from the grid
        if (SGrid.Current.realigningGrid != null)
        {
            //Debug.Log("Reset!");
            for (int x = 0; x < SGrid.Current.Width; x++)
            {
                for (int y = 0; y < SGrid.Current.Height; y++)
                {
                    int tid = originalGrid[x, y];
                    uiArtifactMenus.uiArtifact.GetButton(tid).SetPosition(x, y);
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

    //Rearranging Fragment
    public void FragRearrangeOnClick()
    {
        // Do the rearranging!
        //Debug.Log("Swapped!");
        if (middle == empty)
        {
            AudioManager.Play("Artifact Error");
            return;
        }
        uiArtifactMenus.uiArtifact.TryQueueMoveFromButtonPair(middle, empty);
        uiArtifactMenus.uiArtifact.UpdatePushedDowns(null, null);
        uiArtifactMenus.uiArtifact.DeselectSelectedButton();
        //FragRearrangeOnHoverExit();
    }

    public void FragRearrangeOnHoverEnter()
    {
        rearrangingFragTabAnimator.SetFloat("speed", 4);
        //Preview!
        middle = UIArtifact.GetButton(1, 1);
        empty = uiArtifactMenus.uiArtifact.GetButton(9);
        if (middle.TileIsActive) UIArtifact.SetLightningPos(1, 1);
        middle.SetLightning(true);
        empty.SetLightning(true);        
        //middle.FragLightningPreview(true);
        //empty.FragLightningPreview(true);
    }

    public void FragRearrangeOnHoverExit()
    {
        rearrangingFragTabAnimator.SetFloat("speed", 1);
        //Reset preview
        UIArtifact.DisableLightning(true);
        middle.SetLightning(false);
        empty.SetLightning(false);
        //middle.FragLightningPreview(false);
        //empty.FragLightningPreview(false);
    }

    //Preview Tab

    public void PreviewOnHoverEnter()
    {
        MagiTechArtifact artifact = (MagiTechArtifact) uiArtifactMenus.uiArtifact;
        artifact.SetPreview(true);
        previewTabAnimator.SetBool("isHovered", true);
        previewTabAnimator.SetFloat("speed", previewTabAnimator.GetFloat("speed") * -1);
        artifact.DeselectSelectedButton();
    }

    public void PreviewOnHoverExit()
    {
        MagiTechArtifact artifact = (MagiTechArtifact)uiArtifactMenus.uiArtifact;
        artifact.SetPreview(false);
        previewTabAnimator.SetBool("isHovered", false);
        previewTabAnimator.SetFloat("speed", previewTabAnimator.GetFloat("speed") * -1);
    }

    #endregion
}