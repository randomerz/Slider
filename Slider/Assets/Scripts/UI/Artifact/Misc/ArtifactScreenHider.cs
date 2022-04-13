using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is for the village, to initially hide the first 2 artifact screens
public class ArtifactScreenHider : MonoBehaviour 
{
    public ArtifactScreenAnimator screenAnimator;
    public UIArtifactMenus uiArtifactMenus;

    private List<RectTransform> screens;
    private List<Animator> animators;

    private void Awake() 
    {
        if (!PlayerInventory.Contains("Map", Area.Village))
        {
            Init();
        }
    }

    public void Init()
    {
        screens = new List<RectTransform>(screenAnimator.screens);
        animators = new List<Animator>(screenAnimator.animators);

        screenAnimator.screens.RemoveRange(1, screens.Count - 1);
        screenAnimator.animators.RemoveRange(1, animators.Count - 1);
    }

    public void AddScreens()
    {
        screenAnimator.screens = new List<RectTransform>(screens);
        screenAnimator.animators = new List<Animator>(animators);
    }

    public void AddScreensAndShow()
    {
        AddScreens();
        StartCoroutine(IAddScreensAndShow());
    }

    private IEnumerator IAddScreensAndShow()
    {
        yield return new WaitForSeconds(2.25f); // magic number

        uiArtifactMenus.OpenArtifact();

        // show hint about pressing Q and E here
        Debug.Log("Press [Q] and [E] to switch screens on The Artifact!");
    }
}