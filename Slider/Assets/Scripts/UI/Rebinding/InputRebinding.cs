using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.EventSystems;

public class InputRebinding
{
    public static Action<string> OnRebindStarted;
    public static Action OnRebindCompleted;

    private static bool rebindIsInProgress;
    private static InputActionRebindingExtensions.RebindingOperation currentRebindingOperation;

    public static void StartInteractiveRebindOperation(Control controlToRebind, string localizedName)
    {
        if (rebindIsInProgress)
        {
            return;
        }

        rebindIsInProgress = true;
        OnRebindStarted?.Invoke(localizedName);

        Controls.Bindings.Disable();

        InputAction actionToRebind = Controls.InputActionForControl(controlToRebind);
        currentRebindingOperation = actionToRebind.PerformInteractiveRebinding()
                    .WithTargetBinding(Controls.BindingIndex(controlToRebind))
                    .WithTimeout(5f)
                    .WithControlsExcluding("Mouse")
                    .OnMatchWaitForAnother(0.1f)
                    .Start()
                    .OnComplete((InputActionRebindingExtensions.RebindingOperation rebindingOperation) =>
                    {
                        CompleteRebindingOperation(rebindingOperation);
                        RemoveDuplicateBindings(controlToRebind);
                        
                        // This doesn't work -- for some reason the InputSystemUIInputModule.cs 
                        // doesn't register when you rebind the InputSettings. Even if you change
                        // it to use Player/Move instead of UI/Navigate I couldn't get it to work.

                        // // Also rebind the navigate WASD options if move is rebound
                        // if (controlToRebind.ToString().Contains("Move"))
                        // {
                        //     Controls.Bindings.Disable();

                        //     InputAction alternateActionToRebind = Controls.AlternateInputActionForControl(controlToRebind);
                        //     alternateActionToRebind.ApplyBindingOverride(
                        //         Controls.BindingIndex(controlToRebind),
                        //         actionToRebind.bindings[Controls.BindingIndex(controlToRebind)]
                        //     );
                            
                        //     Controls.Bindings.Enable();
                        // }

                        WriteCurrentBindingsToPlayerPrefs(); // should this be moved to the front of these few?
                        OnRebindCompleted?.Invoke(); // so that if you copy another one it will properly update
                    })
                    .OnCancel((InputActionRebindingExtensions.RebindingOperation rebindingOperation) =>
                    {
                        CompleteRebindingOperation(rebindingOperation);
                        OnRebindCompleted?.Invoke();
                    });
    }

    private static void CompleteRebindingOperation(InputActionRebindingExtensions.RebindingOperation rebindingOperation)
    {
        Controls.Bindings.Enable();
        rebindingOperation.Dispose();
        currentRebindingOperation = null;
        // OnRebindCompleted?.Invoke();
        rebindIsInProgress = false;
    }

    public static void CancelCurrentRebindingOperation()
    {
        if (currentRebindingOperation != null)
        {
            currentRebindingOperation.Cancel();
        }
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
