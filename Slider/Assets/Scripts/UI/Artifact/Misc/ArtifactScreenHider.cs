using System.Collections.Generic;
using UnityEngine;

// This is for the village, to initially hide the first 2 artifact screens
public class ArtifactScreenHider : MonoBehaviour 
{
    public ArtifactScreenAnimator screenAnimator;

    private List<RectTransform> screens;
    private List<Animator> animators;

    private void Awake() 
    {
        Init();
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
}