using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArtifactTab : MonoBehaviour
{
    public UnityEvent OnClick;
    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;

    [SerializeField] private bool isVisible;

    public Animator tabAnimator;
    
    void Start()
    {
        tabAnimator.SetBool("isVisible", isVisible);
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
