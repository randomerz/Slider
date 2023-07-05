using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This is for the village, to initially hide the first 2 artifact screens
public class ArtifactScreenHider : MonoBehaviour 
{
    [SerializeField] private bool debugSkipHiding;

    public ArtifactScreenAnimator screenAnimator;
    public UIArtifactMenus uiArtifactMenus;

    private List<RectTransform> screens;
    private List<Animator> animators;

    public UnityEvent onInventoryEnable;

    // public ArtifactWorldMapGifAnimation gifAnimation;

    private void Awake() 
    {
        if (!PlayerInventory.Contains("Map", Area.Village))
        {
            Init();
        }
    }

    private void Start() 
    {
        // AddScreensAndShow(2);
    }

    private void OnEnable() 
    {
        if (!PlayerInventory.Contains("Map", Area.Village))
            PlayerInventory.OnPlayerGetCollectible += CheckAddInventoryScreen;
    }

    private void OnDisable() 
    {
        PlayerInventory.OnPlayerGetCollectible -= CheckAddInventoryScreen;
    }

    public void Init()
    {
        screens = new List<RectTransform>(screenAnimator.screens);
        animators = new List<Animator>(screenAnimator.animators);

        if (debugSkipHiding)
            return;

        if (!PlayerHasCoffee()) 
        {
            // remove inventory + map
            screenAnimator.screens.RemoveRange(1, screens.Count - 1);
            screenAnimator.animators.RemoveRange(1, animators.Count - 1);
        }
        else
        {
            // remove just map
            screenAnimator.screens.RemoveRange(2, screens.Count - 2);
            screenAnimator.animators.RemoveRange(2, animators.Count - 2);
        }
    }

    public void AddScreens()
    {
        if (debugSkipHiding)
            return;

        screenAnimator.screens = new List<RectTransform>(screens);
        screenAnimator.animators = new List<Animator>(animators);
    }

    public void AddInventoryScreen()
    {
        if (debugSkipHiding)
            return;

        screenAnimator.screens = new List<RectTransform>(screens.GetRange(0, 2));
        screenAnimator.animators = new List<Animator>(animators.GetRange(0, 2));
        onInventoryEnable.Invoke();
    }

    public void AddScreensAndShow()
    {
        AddScreensAndShow(0);
    }

    public void AddScreensAndShow(int screenIndex)
    {
        if (debugSkipHiding)
            return;

        AddScreens();
        uiArtifactMenus.OpenArtifactAndShow(screenIndex, true);
        // StartCoroutine(IAddScreensAndShow(screenIndex));
    }

    // private IEnumerator IAddScreensAndShow(int screenIndex)
    // {
    //     yield return new WaitForSeconds(2.25f); // magic number

    //     uiArtifactMenus.OpenArtifact();

    //     if (screenIndex != 0)
    //     {
    //         yield return new WaitForSeconds(0.3f); // magic number

    //         screenAnimator.SetScreen(screenIndex);
            
    //         // For the gif of the week!
    //         // gifAnimation.ClearAllAreas();
    //         // yield return new WaitForSeconds(0.6f);
    //         // StartCoroutine(gifAnimation.AnimateAllAreas());
    //     }
    // }


    private void CheckAddInventoryScreen(object sender, PlayerInventory.InventoryEvent e)
    {
        if (PlayerHasCoffee() && e.collectible.name == "Coffee")
        {
            AddInventoryScreen();
            PlayerInventory.OnPlayerGetCollectible -= CheckAddInventoryScreen;

            //uiArtifactMenus.OpenArtifactAndShow(0, true);
            // StartCoroutine(IAddScreensAndShow(0));
        }
    }

    private bool PlayerHasCoffee()
    {
        return  PlayerInventory.Contains("Coffee", Area.Village);
    }
}