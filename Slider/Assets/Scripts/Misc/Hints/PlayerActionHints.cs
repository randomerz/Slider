using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionHints : MonoBehaviour
{
    //C: Im trying to write this in a way that is as modular as possible. Good luck me.
    public List<Hint> hintsList;
    private double currHintTimer = 0;
    public InputActionAsset inputActions;

    //Because the hint objects are never actually instantiated I need to do timer logic up here
    void Update()
    {
       foreach(Hint h in hintsList)
       {
            if(h.isInCountdown)
            {
                h.timeUntilTrigger -= Time.deltaTime;
                if(h.timeUntilTrigger < 0)
                {
                    h.Display(inputActions);
                }
            }
       }
    }

    public void TriggerHint(string hint)
    {
        foreach(Hint h in hintsList)
            if(string.Equals(h.hintName, hint))
            {
                h.TriggerHint();
            }
            
    }

    public void SetShouldDisplayFalse(string hint) 
    {
        foreach(Hint h in hintsList)
            if(string.Equals(h.hintName, hint) && h.isInCountdown)
               h.shouldDisplay = false;
    }
}

[System.Serializable]
public class Hint
{ 
    public string hintName;  //used when searching through hints
    public string hintText; //the text of the hint
    public bool canDisableHint; //can this hint be disabled?
    public double timeUntilTrigger; //how long from triggering the hint until it displays
    public bool isInCountdown = false; //is this hint counting down until display? You can set this to true to begin counting down as soon as the scene loads
    public bool shouldDisplay = true; //should this hint display?
    public List<InputRebindButton.Control> controlBinds; //list of control binds to replace in order

    public bool TriggerHint()
    {
        if(timeUntilTrigger >= 0)
        {
            isInCountdown = true;
        }
        return timeUntilTrigger >= 0;
    }

    public void Display(InputActionAsset inputActions) {
        isInCountdown = false;
        if(shouldDisplay)
        {
            UIHints.AddHint(ConvertVariablesToStrings(hintText, inputActions));
        }
    }

    //C: Yoinked from DialogueDisplay, modified to work with rebinds instead of save variables
    private string ConvertVariablesToStrings(string message, InputActionAsset inputActions)
    {
        string rebinds = PlayerPrefs.GetString("rebinds");
        int startIndex = 0;
        int numBinds = 0;
        while (message.IndexOf('<', startIndex) != -1)
        {
            startIndex = message.IndexOf('<', startIndex);
            // case with \<
            if (startIndex != 0 && message[startIndex - 1] == '\\')
            {
                // continue
                startIndex += 1;
                continue;
            }

            int endIndex = message.IndexOf('>', startIndex);
            if (endIndex == -1)
            {
                // no more ends!
                break;
            }
            numBinds++;
            InputRebindButton.Control keybind = controlBinds[numBinds - 1];
            string varResult;
            if (keybind == InputRebindButton.Control.Move_Left || keybind == InputRebindButton.Control.Move_Right || keybind == InputRebindButton.Control.Move_Up || keybind == InputRebindButton.Control.Move_Down)
            {
                var action = inputActions.FindAction("Move");
                varResult = action.bindings[1 + (int)keybind].ToDisplayString().ToUpper();
            }
            else
            {
                var action = inputActions.FindAction(keybind.ToString().Replace("_", string.Empty));
                varResult = action.GetBindingDisplayString().ToUpper();
            }
            message = message.Substring(0, startIndex) + varResult + message.Substring(endIndex + 1);
        }
        return message;
    }

    private void OnDisable()
    {

    }
}