using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class BindingBehaviorComponent : MonoBehaviour
{
    [SerializeField] private Control binding;
    [SerializeField] private UnityEvent behavior;
    [SerializeField] private bool onlyWhenThisIsSelectedUIElement;

    private BindingBehavior bindingBehavior;

    private void OnEnable()
    {
        RegisterBindingBehavior();
    }

    private void OnDisable()
    {
        Controls.UnregisterBindingBehavior(bindingBehavior);
    }

    private void RegisterBindingBehavior()
    {
        string controlName = Controls.InputActionForControl(binding).bindings[Controls.BindingIndex(binding)].path;
        controlName = ControlNameWithoutDeviceType(controlName);

        bindingBehavior = new BindingBehavior(Controls.InputActionForControl(binding),
            (context) =>
            {
                if (context.control.name == controlName)
                {
                    if (!onlyWhenThisIsSelectedUIElement || UINavigationManager.GetCurrentlySelectedGameObject() == gameObject)
                    {
                        behavior?.Invoke();
                    }
                }
            }
        );
        Controls.RegisterBindingBehavior(bindingBehavior);
    }

    private string ControlNameWithoutDeviceType(string controlName)
    {
        return controlName[(controlName.LastIndexOf('/') + 1)..];
    }
}
