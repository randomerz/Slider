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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    //Because the hints are never actually instantiated I need to do timer logic up here
    void Update()
    {
       foreach(Hint h in hintsList)
       {
            if(h.isInCountdown && !(UIManager.GetInstance().isGamePaused))
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
            if(string.Equals(h.hintName, hint))
               h.shouldDisplay = false;
    }
}

[System.Serializable]
public class Hint
{ 
    public string hintName;  //used when searching through hints
    public string hintText; //the text of the hint
    public double timeUntilTrigger; //how long from triggering the hint until it displays
    public bool isInCountdown = false; //is this hint counting down until display? You can set this to true to begin counting down as soon as the scene loads
    public bool shouldDisplay {get; set;} = true; //should this hint display?
    public List<InputRebindButton.Control> controlBinds; //list of controll binds to replace in order

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
            //UIHints.AddHint(hintText);
        }
    }

    //C: Yoinked straight from DialogueDisplay, modified to work with rebinds instead of save variables
    private string ConvertVariablesToStrings(string message, InputActionAsset inputActions)
    {
        Debug.Log(message);
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
            string varName = message.Substring(startIndex + 1, endIndex - startIndex - 1);
            Debug.Log(varName);
            InputRebindButton.Control keybind = controlBinds[numBinds - 1];
            string varResult;
            if (keybind == InputRebindButton.Control.Move_Left || keybind == InputRebindButton.Control.Move_Right || keybind == InputRebindButton.Control.Move_Up || keybind == InputRebindButton.Control.Move_Down)
            {
            /* Our usual method of generating the display string doesn't work well for compositive actions like Move. 
             * We need to get a particular binding for left/right. Left is the first, so it has an index of 1 because the actual compositive itself is index 0.
             * I find this hilariously unintuitive, but I'm not on the Unity dev team making this system, so my opinion doesn't count. We can do 1 + (int) keybind since
             * Control.Left = 0 and Control.Right = 1. 
            */
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
        Debug.Log(message);
        return message;
    }

    private void OnDisable()
    {

    }
}