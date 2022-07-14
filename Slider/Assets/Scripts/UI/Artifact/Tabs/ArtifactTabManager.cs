using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ArtifactTabManager : MonoBehaviour 
{
    public List<ArtifactTab> tabs = new List<ArtifactTab>();

    private bool isRearranging;



    [Header("References")]
    public UIArtifactMenus uiArtifactMenus; // Access UIArtifact through me!

    // Tabs -- this is not a good solution but we only have one set of tabs so it's fine lol
    public Animator rearrangingTabAnimator;
    public Animator rearrangingFragTabAnimator;
    public ArtifactTab fragRealignTab;
    public ArtifactTab RealignTab;
    public ArtifactTab saveTab;
    public ArtifactTab loadTab;
    private int[,] originalGrid;
    public Sprite saveTabSprite;
    public Sprite loadTabSprite;
    public Sprite saveEmptyTabSprite;
    public Sprite loadEmptyTabSprite;

    private ArtifactTileButton empty;
    private ArtifactTileButton middle;
    public void SetCurrentScreen(int screenIndex)
    {
        if (PlayerInventory.Contains("Scroll of Realigning", Area.Desert)
            && SGrid.Current.GetActiveTiles().Count == SGrid.Current.GetTotalNumTiles()
            && SGrid.GetNumButtonCompletions() != SGrid.Current.GetTotalNumTiles())
        {
            RealignTab.SetIsVisible(screenIndex == RealignTab.homeScreen);
            saveTab.SetIsVisible(false);
            loadTab.SetIsVisible(false);
        }
        else if (PlayerInventory.Contains("Scroll of Realigning", Area.Desert))
        {
            saveTab.SetIsVisible(screenIndex == saveTab.homeScreen);
            loadTab.SetIsVisible(screenIndex == loadTab.homeScreen);
            SetSaveLoadTabSprites(SGrid.Current.HasRealigningGrid());
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
            RealignTab.SetIsVisible(false);
            fragRealignTab.SetIsVisible(false);
            saveTab.SetIsVisible(false);
            loadTab.SetIsVisible(false);
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
            uiArtifactMenus.uiArtifact.DeselectCurrentButton();
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
        uiArtifactMenus.uiArtifact.FragRealignCheckAndSwap(middle, empty);
        uiArtifactMenus.uiArtifact.UpdatePushedDowns(null, null);
        uiArtifactMenus.uiArtifact.DeselectCurrentButton();
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
    #endregion
}