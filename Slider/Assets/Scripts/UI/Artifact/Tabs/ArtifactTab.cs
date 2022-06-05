using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArtifactTab : MonoBehaviour
{
    public UnityEvent OnClick;
    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;

    private bool isVisible; // for when you are on a separate screen
    [SerializeField] private bool isActive; // if the tab works or not
    public int homeScreen = 0; // default screen = artifact screen

    public Animator tabAnimator;
    
    void Start()
    {
        gameObject.SetActive(isActive); // this is bad but should work for now? no it's bad >:(
        UpdateVisibility();
    }

    public bool GetIsVisible()
    {
        return isVisible && isActive;
    }

    public void SetIsVisible(bool value)
    {
        isActive = value;
        gameObject.SetActive(isActive);
        isVisible = value;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        tabAnimator.SetBool("isVisible", GetIsVisible());
    }

    public void Click()
    {
        tabAnimator.SetTrigger("click");
        AudioManager.Play("UI Click");

        OnClick?.Invoke();
    }

    public void HoverEnter()
    {
        OnHoverEnter?.Invoke();
    }

    public void HoverExit()
    {
        OnHoverExit?.Invoke();
    }
}
