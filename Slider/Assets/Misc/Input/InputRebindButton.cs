using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
// using MyBox;
using UnityEngine.InputSystem;
using TMPro;

public class InputRebindButton : MonoBehaviour
{
    [SerializeField] private Control keybind;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private int maxLineLength = 19;
    [SerializeField] private bool isPauseMenu;

    private void OnEnable()
    {
        Initialize();
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
    /// Remapping movement keys works differently, so we need a separate method for it.
    /// Performs the same functionality as IRemapKeybind.
    /// </summary>
    /// <returns></returns>
    private IEnumerator IRemapMovementKeybind()
    {
        Debug.Log("Testing...");
        var action = inputActions.FindAction("Move");
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
        Debug.Log("Testing Done!");
    }

    /// <summary>
    /// Uses PerformInteractiveRebinding to rebind the button's keybind, then saves all keybinds to PlayerPrefs.
    /// Also updates the text of the button in the process.
    /// </summary>
    /// <returns></returns>
    private IEnumerator IRemapKeybind()
    {
        var action = inputActions.FindAction(keybind.ToString().Replace("_", string.Empty));
        action.Disable();
        var rebindOperation = action.PerformInteractiveRebinding()
                    // To avoid accidental input from mouse motion
                    .WithControlsExcluding("Mouse")
                    .OnMatchWaitForAnother(0.1f)
                    .Start()
                    .OnComplete((InputActionRebindingExtensions.RebindingOperation op) => UpdateButtonText());
        
        yield return new WaitWhile(() => !rebindOperation.completed);
        
        rebindOperation.Dispose(); // Stop memory leaks
        action.Enable();

        // Save our keybinds to PlayerPrefs so we can load them when the actual game starts
        var rebinds = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }

    /// <summary>
    /// Loads bindings from PlayerPrefs and sets button text accordingly. 
    /// Called by MainMenuManager to setup the buttons when entering the options menu.
    /// </summary>
    public void Initialize()
    {   
        // Load our keybinds from PlayerPrefs
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!String.IsNullOrEmpty(rebinds))
        {
            inputActions.LoadBindingOverridesFromJson(rebinds);
        }

        UpdateButtonText();
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
            var action = inputActions.FindAction("Move");
            //buttonText.text = buttonText.text = $"{keybind.ToString().ToUpper().Replace("_", " ")}: {action.bindings[1 + (int)keybind].ToDisplayString().ToUpper().Replace("PRESS ", "").Replace(" ARROW", "")}";
            buttonText.text = buttonText.text = ShrinkFontSizeIfNeeded(keybind.ToString().ToUpper().Replace("_", " ") + ": " , action.bindings[1 + (int)keybind].ToDisplayString().ToUpper().Replace("PRESS ", "").Replace(" ARROW", ""));

            // Save our bindings
            var rebinds = inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);

            Player.LoadBindings();
        }
        else
        {
            Debug.Log("update text");
            var action = inputActions.FindAction(keybind.ToString().Replace("_", string.Empty));
            string display = keybind.ToString().Replace("_", " ");
            int upperInd = 0;
            for (int i=1; i < display.Length; i++)
            {
                if (char.IsUpper(display.ToCharArray()[i]))
                    upperInd = i;
            }
            if(upperInd > 0) {
                 display = display.Substring(0, upperInd) + ' ' + display.Substring(upperInd);
            }
            Debug.Log(display);
           // buttonText.text = buttonText.text = $"{display.ToUpper()}: {action.GetBindingDisplayString().ToUpper().Replace("PRESS ", "").Replace(" ARROW", "")}";
            buttonText.text = buttonText.text = ShrinkFontSizeIfNeeded(display.ToUpper() + ": " , action.GetBindingDisplayString().ToUpper().Replace("PRESS ", "").Replace(" ARROW", ""));
            var rebinds = inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
            
            if (keybind == Control.Action || keybind == Control.CycleEquip)
            {
                PlayerAction.LoadBindings();
            } 
            else 
            {
                UIManager.LoadBindings();
                ShopManager.LoadBindings(); // for Ocean shop UI
                UIArtifactMenus.LoadBindings(); // for artiface menus
                MainMenuManager.LoadBindings(); // for main menu
            }
        }
    }

    private string ShrinkFontSizeIfNeeded(string s1, string s2) 
        {
            if(isPauseMenu)
                s1 = "";
            int length = (s1 + s2).Length;
            if(length > maxLineLength) {
            buttonText.fontSize = 9.5f;
            return s1 + "\n" + s2;
            }
            else {
            buttonText.fontSize = 14;
            return s1 + s2;
            }
        }

   

    public enum Control
    {
        Move_Up = 0,
        Move_Down = 1,
        Move_Left = 2,
        Move_Right = 3,
        Action,
        CycleEquip,
        OpenArtifact,
        Pause
    }
}
    