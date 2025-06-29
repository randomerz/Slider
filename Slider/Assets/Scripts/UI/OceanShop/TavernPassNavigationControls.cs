using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernPassNavigationControls : MonoBehaviour
{
    public TavernPassManager tavernPassManager;

    private Vector2 lastInput;
    private BindingBehavior bindingBehavior;

    private void OnEnable() {
        bindingBehavior = Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.Navigate, context => {
            Vector2 input = context.ReadValue<Vector2>();
            if (input == lastInput) return; // Ignore if input hasn't changed
            lastInput = input;
            
            if (input.x < 0)
                tavernPassManager.DecrementButton();
            else if (input.x > 0)
                tavernPassManager.IncrementButton();
        });
    }

    private void OnDisable() {
        // According to lord of movement travis these happen for free when disabled
        Controls.UnregisterBindingBehavior(bindingBehavior);
    }
}
