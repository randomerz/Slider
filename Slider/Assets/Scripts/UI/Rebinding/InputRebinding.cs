using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class InputRebinding
{
    public static Action<Control> OnRebindStarted;
    public static Action OnRebindCompleted;

    private static bool rebindIsInProgress;

    public static void StartInteractiveRebindOperation(Control controlToRebind)
    {
        if (rebindIsInProgress)
        {
            return;
        }

        rebindIsInProgress = true;
        OnRebindStarted?.Invoke(controlToRebind);

        Controls.Bindings.Disable();

        InputAction actionToRebind = Controls.InputActionForControl(controlToRebind);
        actionToRebind.PerformInteractiveRebinding()
                      .WithTargetBinding(Controls.BindingIndex(controlToRebind))
                      .WithTimeout(5f)
                      .WithControlsExcluding("Mouse")
                      .OnMatchWaitForAnother(0.1f)
                      .Start()
                      .OnComplete((InputActionRebindingExtensions.RebindingOperation rebindingOperation) =>
                      {
                          CompleteRebindingOperation(rebindingOperation);
                          RemoveDuplicateBindings(controlToRebind);
                          WriteCurrentBindingsToPlayerPrefs();
                      })
                      .OnCancel((InputActionRebindingExtensions.RebindingOperation rebindingOperation) =>
                      {
                          CompleteRebindingOperation(rebindingOperation);
                      });
    }

    private static void CompleteRebindingOperation(InputActionRebindingExtensions.RebindingOperation rebindingOperation)
    {
        Controls.Bindings.Enable();
        rebindingOperation.Dispose();
        OnRebindCompleted?.Invoke();
        rebindIsInProgress = false;
    }

    private static void RemoveDuplicateBindings(Control controlThatWasJustRebound)
    {
        string newBindingForControl = Controls.InputActionForControl(controlThatWasJustRebound).bindings[Controls.BindingIndex(controlThatWasJustRebound)].effectivePath;

        Control[] allControls = (Control[])Enum.GetValues(typeof(Control));
        allControls.Where(control => control != controlThatWasJustRebound)
                   .ToList()
                   .ForEach(otherControl =>
                   {
                       InputAction otherInputAction = Controls.InputActionForControl(otherControl);
                       string bindingForOtherControl = otherInputAction.bindings[Controls.BindingIndex(otherControl)].effectivePath;
                       if (bindingForOtherControl == newBindingForControl)
                       {
                           otherInputAction.ApplyBindingOverride(Controls.BindingIndex(otherControl), "");
                       }
                   });
    }

    private static void WriteCurrentBindingsToPlayerPrefs()
    {
        PlayerPrefs.SetString(Controls.PLAYER_PREFS_REBINDS_KEY, Controls.Bindings.SaveBindingOverridesAsJson());
        Controls.LoadBindings();
    }
}
