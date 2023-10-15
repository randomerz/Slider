using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

[Obsolete("This class has been replaced by RebindingButton.cs and should no longer be used.")]
public class InputRebindButton : MonoBehaviour
{
    [SerializeField] private Control keybind;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private int maxLineLength = 19;
    [SerializeField] private bool isPauseMenu;

    private void OnEnable()
    {
        UpdateButtonText();
    }

    public void RemapKeybind()
    {
        if (keybind == Control.Move_Left || keybind == Control.Move_Right || keybind == Control.Move_Up || keybind == Control.Move_Down)
        {   
            StartCoroutine(IRemapMovementKeybind());
        } else
        {
            StartCoroutine(IRemapKeybind());
        }
    }

    /// <summary>
    /// Remapping movement keys works differently, so we need a separate method for it. Performs the same functionality as IRemapKeybind.
    /// </summary>
    /// <returns></returns>
    private IEnumerator IRemapMovementKeybind()
    {
        var action = Controls.Bindings.FindAction("Move");
        action.Disable();

        var rebindOperation = action.PerformInteractiveRebinding()
                    // To avoid accidental input from mouse motion
                    .WithControlsExcluding("Mouse")
                    .OnMatchWaitForAnother(0.1f)
                    .WithTargetBinding(1 + (int)keybind)
                    .OnComplete((InputActionRebindingExtensions.RebindingOperation op) => UpdateButtonText())
                    .Start();

        yield return new WaitWhile(() => !rebindOperation.completed);

        rebindOperation.Dispose(); // Stop memory leaks
        action.Enable();
    }

    /// <summary>
    /// Uses PerformInteractiveRebinding to rebind the button's keybind, then saves all keybinds to PlayerPrefs.
    /// Also updates the text of the button in the process.
    /// </summary>
    /// <returns></returns>
    private IEnumerator IRemapKeybind()
    {
        var action = Controls.Bindings.FindAction(keybind.ToString().Replace("_", string.Empty));

        action.Disable();
        var rebindOperation = action.PerformInteractiveRebinding()
                    .WithControlsExcluding("Mouse")
                    .OnMatchWaitForAnother(0.1f)
                    .Start()
                    .OnComplete((InputActionRebindingExtensions.RebindingOperation op) => UpdateButtonText());
        
        yield return new WaitWhile(() => !rebindOperation.completed);
        
        rebindOperation.Dispose(); // Stop memory leaks
        action.Enable();

        PlayerPrefs.SetString(Controls.PLAYER_PREFS_REBINDS_KEY, Controls.Bindings.SaveBindingOverridesAsJson());
        Controls.LoadBindings();

    }

    private void UpdateButtonText()
    {
        if (keybind == Control.Move_Left || keybind == Control.Move_Right || keybind == Control.Move_Up || keybind == Control.Move_Down)
        {
            /* Our usual method of generating the display string doesn't work well for compositive actions like Move. 
             * We need to get a particular binding for left/right. Left is the first, so it has an index of 1 because the actual compositive itself is index 0.
             * I find this hilariously unintuitive, but I'm not on the Unity dev team making this system, so my opinion doesn't count. We can do 1 + (int) keybind since
             * Control.Left = 0 and Control.Right = 1. 
            */
            var action = Controls.Bindings.FindAction("Move");
            buttonText.text = ShrinkFontSizeIfNeeded(keybind.ToString().ToUpper().Replace("_", " ") 
                + ": " , action.bindings[1 + (int)keybind].ToDisplayString()
                .ToUpper().Replace("PRESS ", "").Replace(" ARROW", ""));
        }
        else
        {
            var action = Controls.Bindings.FindAction(keybind.ToString().Replace("_", string.Empty));
            string display = keybind.ToString().Replace("_", " ");
            int upperInd = 0;
            for (int i = 1; i < display.Length; i++)
            {
                if (char.IsUpper(display.ToCharArray()[i]))
                    upperInd = i;
            }
            if (upperInd > 0) {
                 display = display.Substring(0, upperInd) + ' ' + display.Substring(upperInd);
            }

            buttonText.text = ShrinkFontSizeIfNeeded(display.ToUpper() 
                + ": " , Controls.BindingDisplayString(keybind).ToUpper().Replace("PRESS ", "").Replace(" ARROW", ""));
        }

        PlayerPrefs.SetString(Controls.PLAYER_PREFS_REBINDS_KEY, Controls.Bindings.SaveBindingOverridesAsJson());
        Controls.LoadBindings();
    }

    private string ShrinkFontSizeIfNeeded(string s1, string s2) 
    {
        if(isPauseMenu)
            s1 = "";
        int length = (s1 + s2).Length;
        if(length > maxLineLength) 
        {
            buttonText.fontSize = 9.5f;
            return s1 + "\n" + s2;
        }
        else 
        {
            buttonText.fontSize = 14;
            return s1 + s2;
        }
    }
}
    