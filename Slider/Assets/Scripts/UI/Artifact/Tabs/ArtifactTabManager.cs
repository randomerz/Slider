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
    public ArtifactTab fragRealignTab;
    public ArtifactTab RealignTab;
    public ArtifactTab saveTab;
    public ArtifactTab loadTab;
    private int[,] originalGrid;
    public Sprite saveTabSprite;
    public Sprite loadTabSprite;
    public Sprite saveEmptyTabSprite;
    public Sprite loadEmptyTabSprite;

    public void SetCurrentScreen(int screenIndex)
    {
        Debug.Log("Checked stuff");
        if (PlayerInventory.Contains("Scroll Frag", Area.Desert)
            && !PlayerInventory.Contains("Scroll of Realigning", Area.Desert))
        {
            fragRealignTab.SetIsVisible(screenIndex == fragRealignTab.homeScreen);
        }
        else if (PlayerInventory.Contains("Scroll of Realigning", Area.Desert)
            && SGrid.current.GetActiveTiles().Count == SGrid.current.GetTotalNumTiles()
            && SGrid.GetNumButtonCompletions() != SGrid.current.GetTotalNumTiles())
        {
            RealignTab.SetIsVisible(screenIndex == RealignTab.homeScreen);
        }
        else if (PlayerInventory.Contains("Scroll of Realigning", Area.Desert)
                 && SGrid.GetNumButtonCompletions() != SGrid.current.GetTotalNumTiles())
        {
            saveTab.SetIsVisible(screenIndex == saveTab.homeScreen);
            loadTab.SetIsVisible(screenIndex == loadTab.homeScreen);
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
            Debug.Log("Rearranged!");


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
        // flash tiles white
        //Debug.Log("Tried to save!");
        SGrid.current.SaveSaveGrid();
        uiArtifactMenus.uiArtifact.FlickerAllOnce();
        saveTab.GetComponentInChildren<Image>().sprite = saveTabSprite;
        loadTab.GetComponentInChildren<Image>().sprite = loadTabSprite;
    }

    // Load tab

    public void LoadOnClick()
    {
        if (SGrid.current.saveGrid != null)
        {
            // Do the rearranging!
            SGrid.current.LoadSaveGrid();
            Debug.Log("Loaded!");


            UIEffects.FadeFromWhite();
            CameraShake.Shake(1.5f, 0.75f);
            AudioManager.Play("Slide Explosion");

            saveTab.GetComponentInChildren<Image>().sprite = saveEmptyTabSprite;
            loadTab.GetComponentInChildren<Image>().sprite = loadEmptyTabSprite;
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

    public void LoadOnHoverEnter(UIArtifact uiartifact)
    {
        // set it to saved
        //get the realignGrid, put the button stuff in the order based on that, then call bggrid set
        if(SGrid.current.saveGrid != null)
        {
            //Debug.Log("Previewed!");
            originalGrid = new int[SGrid.current.width, SGrid.current.height];
            for (int x = 0; x < SGrid.current.width; x++)
            {
                for (int y = 0; y < SGrid.current.height; y++)
                {
                    //Debug.Log(SGrid.current.saveGrid[x, y] + " button array index: " + (SGrid.current.saveGrid[x, y] - 1) + " " + x + " " + y);
                    originalGrid[x, y] = SGrid.current.GetGrid()[x, y].islandId;
                    uiartifact.buttons[SGrid.current.saveGrid[x, y] - 1].SetPosition(x, y);
                }
            }
        }
    }

    public void LoadOnHoverExit(UIArtifact uiartifact)
    {
        // reset
        //get the id's from the grid
        if (SGrid.current.saveGrid != null)
        {
            Debug.Log("Reset!");
            for (int x = 0; x < SGrid.current.width; x++)
            {
                for (int y = 0; y < SGrid.current.height; y++)
                {
                    uiartifact.buttons[originalGrid[x, y] - 1].SetPosition(x, y);
                }
            }
        }
    }

    //Rearranging Fragment

    public void FragRearrangeOnClick()
    {
        if (isRearranging)
            return;
        isRearranging = true;

        StartCoroutine(IFragRearrangeOnClick());
    }

    private IEnumerator IFragRearrangeOnClick()
    {
        UIManager.InvokeCloseAllMenus();
        UIManager.canOpenMenus = false;

        CameraShake.ShakeIncrease(2, 1);
        AudioManager.Play("Slide Explosion"); // TODO: fix sfx

        yield return new WaitForSeconds(0.5f);
        AudioManager.Play("Slide Explosion");

        UIEffects.FlashWhite(callbackMiddle: () => {
            // Do the rearranging!
            Debug.Log("Rearranged!");


            UIManager.canOpenMenus = true;
            isRearranging = false;
        }, speed: 0.5f);

        yield return new WaitForSeconds(1.5f);

        CameraShake.Shake(2, 1);
    }

    public void FragRearrangeOnHoverEnter()
    {
        rearrangingTabAnimator.SetFloat("speed", 2);
    }

    public void FragRearrangeOnHoverExit()
    {
        rearrangingTabAnimator.SetFloat("speed", 1);
    }
    #endregion
}