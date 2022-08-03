using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArtifactTab : MonoBehaviour
{
    public UnityEvent OnClick;
    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;

    [SerializeField] protected bool isVisible; // for when you are on a separate screen
    [SerializeField] protected bool isActive; // if the tab works or not
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

    public virtual void SetIsVisible(bool value)
    {
        if (!value && isActive)
        {
            StartCoroutine(SetVisibleThenDisable());
        }
        else
        {
            isActive = value;
            isVisible = value;
            gameObject.SetActive(isActive);
            UpdateVisibility();
        }
    }

    protected void UpdateVisibility()
    {
        tabAnimator.SetBool("isVisible", GetIsVisible());
    }

    protected virtual IEnumerator SetVisibleThenDisable()
    {
        isVisible = false;
        isActive = false;
        UpdateVisibility();
        yield return new WaitForSeconds(.5f);
        gameObject.SetActive(false);
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
