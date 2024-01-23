using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactScreenAnimator : MonoBehaviour
{
    // one day we will replace the function with an event
    // public class ScreenChangeEvent : System.EventArgs {
    //     int screenIndex;
    // }

    // public static System.EventHandler<ScreenChangeEvent> OnScreenChange;

    private int currentScreenIndex;
    private int targetScreenIndex;

    public float duration;

    private Coroutine switchCouroutine;

    [Header("References")]
    public List<RectTransform> screens;
    public List<Animator> animators;
    public Animator rightArrowAnimator;
    public Animator leftArrowAnimator;
    public ArtifactTabManager tabManager;
    public UIArtifactInventory inventory;

    private void OnEnable() 
    {
        currentScreenIndex = 0;
        targetScreenIndex = 0;

        SetScreensActive(false);
        screens[0].gameObject.SetActive(true);
        animators[0].SetBool("isVisible", true);

        UpdateArrowVisibility(true);
    }

    private void OnDisable() 
    {
        StopAllCoroutines();
        switchCouroutine = null;
    }

    private void Update() 
    {
        if (currentScreenIndex != targetScreenIndex && switchCouroutine == null)
        {
            switchCouroutine = StartCoroutine(SwitchScreens(targetScreenIndex));
        }
    }

    public void NextScreen()
    {
        if (targetScreenIndex + 1 >= screens.Count)
        {
            return;
        }
        SetScreen(targetScreenIndex + 1);
    }

    public void PrevScreen()
    {
        if (targetScreenIndex - 1 < 0)
        {
            return;
        }
        SetScreen(targetScreenIndex - 1);
    }

    public void SetScreen(int index)
    {
        if (index < 0 || index >= screens.Count)
        {
            Debug.LogError("Screen index out of bounds!");
        }

        targetScreenIndex = index;
    }

    public void CycleScreen()
    {
        targetScreenIndex = (targetScreenIndex + 1) % screens.Count;
        SetScreen(targetScreenIndex);
    }

    private void UpdateArrowVisibility(bool immediate)
    {
        rightArrowAnimator.SetBool("isVisible", targetScreenIndex < screens.Count - 1);
        rightArrowAnimator.SetBool("immediate", immediate);
        leftArrowAnimator.SetBool("isVisible", targetScreenIndex > 0);
        leftArrowAnimator.SetBool("immediate", immediate);
        
        tabManager?.SetCurrentScreen(targetScreenIndex);
    }

    private IEnumerator SwitchScreens(int target)
    {
        screens[target].gameObject.SetActive(true);
        animators[target].SetBool("isVisible", true);
        UpdateArrowVisibility(false);

        if (currentScreenIndex < target)
        {
            // screens move left
            animators[currentScreenIndex].SetTrigger("rightOut");
            animators[target].SetTrigger("leftIn");

            rightArrowAnimator.SetTrigger("bump");
        }
        else
        {
            animators[currentScreenIndex].SetTrigger("leftOut");
            animators[target].SetTrigger("rightIn");

            leftArrowAnimator.SetTrigger("bump");
        }

        yield return new WaitForSeconds(duration);
        
        OnScreenChange(target, currentScreenIndex);

        currentScreenIndex = target;

        SetScreensActive(false);
        screens[currentScreenIndex].gameObject.SetActive(true);
        animators[currentScreenIndex].SetBool("isVisible", true);
        switchCouroutine = null;

        if (targetScreenIndex != currentScreenIndex)
            SwitchScreens(targetScreenIndex);
    }

    private void SetScreensActive(bool value)
    {
        for (int i = 0; i < screens.Count; i++)
        {
            screens[i].gameObject.SetActive(value);
            if (value) animators[i].SetBool("isVisible", value);
        }
    }

    private void OnScreenChange(int index, int prevIndex)
    {
        //inventory
        if(index == 1)
        {
            if(prevIndex == 0)
            {
                inventory.TrySelectLeftmostSelectible();
            }

            if(prevIndex == 2)
            {
                inventory.TrySelectRightmostSelectible();
            }

        }

        // map screen
        if (index == 2)
        {
            UIArtifactWorldMap.GetInstance().UpdateAreaStatuses();
        }
    }
}
