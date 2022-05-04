using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArtifactTabManager : MonoBehaviour 
{
    public List<ArtifactTab> tabs = new List<ArtifactTab>();

    private bool isRearranging;



    [Header("References")]
    public UIArtifactMenus uiArtifactMenus; // Access UIArtifact through me!

    // Tabs -- this is not a good solution but we only have one set of tabs so it's fine lol
    public Animator rearrangingTabAnimator;

    public void SetCurrentScreen(int screenIndex)
    {
        foreach (ArtifactTab t in tabs)
        {
            t.SetIsVisible(screenIndex == t.homeScreen);
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
        uiArtifactMenus.uiArtifact.FlickerAllOnce();
    }

    // Load tab

    public void LoadOnClick()
    {
        // Do the rearranging!
        SGrid.current.SetGrid(new int[,] {{7, 4, 1},
                                          {8, 5, 2},
                                          {9, 6, 3}});
        Debug.Log("Rearranged!");

        
        UIEffects.FadeFromWhite();
        CameraShake.Shake(1.5f, 0.75f);
        AudioManager.Play("Slide Explosion");

        // StartCoroutine(ILoadOnClick());
    }

    private IEnumerator ILoadOnClick()
    {
        yield return null;
    }

    public void LoadOnHoverEnter()
    {
        // set it to saved
    }

    public void LoadOnHoverExit()
    {
        // reset
    }

    #endregion
}