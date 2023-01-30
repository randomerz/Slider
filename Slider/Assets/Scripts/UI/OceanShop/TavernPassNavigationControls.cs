using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernPassNavigationControls : MonoBehaviour
{
    public TavernPassManager tavernPassManager;

    private BindingBehavior leftBindingBehavior;
    private BindingBehavior rightBindingBehavior;

    private void OnEnable() {
        leftBindingBehavior = Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Move, context => {
            if (context.ReadValue<Vector2>().x < 0)
                tavernPassManager.DecrementButton();
        });

        rightBindingBehavior = Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Move, context => {
            if (context.ReadValue<Vector2>().x > 0)
                tavernPassManager.IncrementButton();
        });

    }

    private void OnDisable() {
        // According to lord of movement travis these happen for free when disabled
        Controls.UnregisterBindingBehavior(leftBindingBehavior);
        Controls.UnregisterBindingBehavior(rightBindingBehavior);
    }
}
