using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ArtifactTab : MonoBehaviour
{
    public UnityEvent OnClick;
    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;

    [SerializeField] protected bool isVisible; // for when you are on a separate screen
    [SerializeField] protected bool isActive; // if the tab works or not
    public int homeScreen = 0; // default screen = artifact screen

    public Animator tabAnimator;
    public Image controllerSelectedImage;

    private bool selected;
    
    void Start()
    {
        gameObject.SetActive(isActive); // this is bad but should work for now? no it's bad >:(
        UpdateVisibility();
    }

    private void OnEnable()
    {
        Controls.OnControlSchemeChanged += OnControlSchemeChanged;
    }

    private void OnDisable()
    {
        Controls.OnControlSchemeChanged -= OnControlSchemeChanged;
    }

    private void OnControlSchemeChanged()
    {
        if(Controls.CurrentControlScheme == Controls.CONTROL_SCHEME_KEYBOARD_MOUSE)
        {
            Deselect();
        }
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
        yield return new WaitForSeconds(.25f);
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

    public void Select()
    {
        if(Controls.CurrentControlScheme == Controls.CONTROL_SCHEME_CONTROLLER)
        {
            print("selected");
            controllerSelectedImage.enabled = true;
            selected = true;
            OnHoverEnter?.Invoke();
        }
    }

    public void Deselect()
    {
        controllerSelectedImage.enabled = false;
        if(selected)
            OnHoverExit?.Invoke();
    }
}
