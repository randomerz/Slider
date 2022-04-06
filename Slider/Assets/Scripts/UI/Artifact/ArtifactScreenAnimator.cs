using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactScreenAnimator : MonoBehaviour
{
    public List<RectTransform> screens;
    public List<Animator> animators;
    private int currentScreenIndex;
    private int targetScreenIndex;

    public float duration;

    private Coroutine switchCouroutine;

    private void OnEnable() 
    {
        currentScreenIndex = 0;
        targetScreenIndex = 0;
        animators[0].SetBool("isVisible", true);

        for (int i = 1; i < screens.Count; i++)
        {
            animators[i].SetBool("isVisible", false);
        }
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

    private IEnumerator SwitchScreens(int target)
    {
        screens[target].gameObject.SetActive(true);
        animators[target].SetBool("isVisible", true);

        if (currentScreenIndex < target)
        {
            // screens move left
            animators[currentScreenIndex].SetTrigger("rightOut");
            animators[target].SetTrigger("leftIn");
        }
        else
        {
            animators[currentScreenIndex].SetTrigger("leftOut");
            animators[target].SetTrigger("rightIn");
        }

        yield return new WaitForSeconds(duration);

        currentScreenIndex = target;

        SetScreensActive(false);
        screens[currentScreenIndex].gameObject.SetActive(true);
        animators[currentScreenIndex].SetBool("isVisible", true);

        switchCouroutine = null;

        if (targetScreenIndex != currentScreenIndex)
            SwitchScreens(targetScreenIndex);
    }

    public void SetScreen(int index)
    {
        if (index < 0 || index >= screens.Count)
        {
            Debug.LogError("Screen index out of bounds!");
        }

        targetScreenIndex = index;
    }

    private void SetScreensActive(bool value)
    {
        for (int i = 0; i < screens.Count; i++)
        {
            screens[i].gameObject.SetActive(value);
            animators[i].SetBool("isVisible", true);
        }
    }

}
