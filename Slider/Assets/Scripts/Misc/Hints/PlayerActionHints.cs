using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class PlayerActionHints : MonoBehaviour, ISavable
{
    public List<Hint> hintsList;

    public UnityEvent ArtifactOpen;
    public UnityEvent PlayerMove;
    public UnityEvent PlayerAction;
    public UnityEvent PlayerCycle;
    public UnityEvent PlayerAltViewHold;

    private void Awake() {
        Controls.RegisterBindingBehavior(this, Controls.Bindings.UI.OpenArtifact, context => {
            if (SGrid.Current.GetArea() != Area.Village || SaveSystem.Current.GetBool("villageTouchedGrass"))
                ArtifactOpen?.Invoke();
        });
        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Action, context => PlayerAction?.Invoke());
        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.CycleEquip, context => PlayerCycle?.Invoke());
        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Move, context => PlayerMove?.Invoke());
        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.AltViewHold, context => PlayerAltViewHold?.Invoke());
    }

    public void Save() {
        foreach (Hint h in hintsList)
            h.Save();
    }

    public void Load(SaveProfile profile) {
        foreach (Hint h in hintsList)
        {
            h.Load(profile);
        }
    }

    private void OnEnable()
    {
        Load(SaveSystem.Current);
    }

    void Update()
    {
       foreach (Hint h in hintsList)
       {
            if (h.isInCountdown && h.shouldDisplay)
            {
                h.timeUntilTrigger -= Time.deltaTime;
                if (h.timeUntilTrigger < 0)
                {
                    h.Display();
                }
            }
       }
    }

    //C: triggers the countdown for the given hint to begin
    public void TriggerHint(string hint)
    {
        foreach (Hint h in hintsList)
            if (string.Equals(h.hintData.hintName, hint))
                h.TriggerHint();
    }

    //C: Disables the given hint (if the hint can be disabled)
    public void DisableHint(string hint) 
    {
        foreach (Hint h in hintsList)
            if (string.Equals(h.hintData.hintName, hint) && h.canDisableHint)
                h.DisableHint();
    }

    //C: Allows the given hint to be disabled. Needed because multiple hints can be tied to the same action/button
    // Hints always become disablable once triggered, so only use this if you want to make a hint disablable earlier
    // for example, the "press E to pick up items" hint is disablable when tile 4 is collected, but the hint
    // itself isn't triggered until you talk to Kevin
    // This is almost always just for convienience for repeat playthroughs
    public void EnableDisabling(string hint) 
    {
        foreach (Hint h in hintsList)
            if (string.Equals(h.hintData.hintName, hint))
               h.canDisableHint = true;
    }

    public void DebugLog(string s){
        Debug.Log(s);
    }

}

[System.Serializable]
public class HintData
{
    public string hintName;  //used when searching through hints
    public string hintText; //the text of the hint
    public string controllerHintText;
    public List<Control> controlBinds; //list of control binds to replace in order


    public string GetFormattedHintText()
    {
        string hintTextToDisplay = CheckConvertToControllerHintText(hintText);
        string ret = ReplacePlaceholdersWithControlBindings(hintTextToDisplay);
        return ret;
    }

    private string CheckConvertToControllerHintText(string message)
    {
        if (!controllerHintText.Equals("") && Player.GetInstance().GetCurrentControlScheme() == "Controller")
            return controllerHintText;
        else
            return message;
    }

    private string ReplacePlaceholdersWithControlBindings(string message)
    {
        Queue<Control> bindingsToPlace = new(controlBinds);

        // We sometimes have escaped angle braces that we need to remove
        message.Replace("/<", "<")
               .Replace("/>", ">");

        while (message.Contains("<"))
        {
            int startOfPlaceholder = message.IndexOf("<");
            int endOfPlaceholder = message.IndexOf(">");
            string placeholder = message.Substring(startOfPlaceholder, endOfPlaceholder - startOfPlaceholder + 1);

            string controlBinding = Controls.BindingDisplayString(bindingsToPlace.Dequeue())
                                            .ToUpper()
                                            .Replace("PRESS ", "")
                                            .Replace(" ARROW", "");

            message = message.Replace(placeholder, controlBinding);
        }

        message = message.Replace("W/A/S/D", "WASD");

        return message;
    }
} 

[System.Serializable]
public class Hint
{ 
    public HintData hintData;
    public bool canDisableHint; //can this hint be disabled?
    public double timeUntilTrigger; //how long from triggering the hint until it displays
    public bool isInCountdown = false; //is this hint counting down until display? 
    public bool triggerOnLoad = false; //should this hint start counting down when the scene is loaded?
    public bool shouldDisplay = true; //should this hint display?
    public bool hasBeenCompleted; //has this hint been completed?
    public bool hasBeenAddedToDisplay; //has this hint been displayed?

    public void Save()
    {
        SaveSystem.Current.SetBool("Hint " + hintData.hintName, shouldDisplay);
        SaveSystem.Current.SetBool("HintCountdown " +  hintData.hintName, isInCountdown);
        SaveSystem.Current.SetBool("HintComplete " +  hintData.hintName, hasBeenCompleted);
        hasBeenAddedToDisplay = false;
    }

    public void Load(SaveProfile profile)
    {
        shouldDisplay = profile.GetBool("Hint " +  hintData.hintName, true);
        isInCountdown = profile.GetBool("HintCountdown " +  hintData.hintName);
        hasBeenCompleted = profile.GetBool("HintComplete " +  hintData.hintName);
        SetBools();
    }
    
    public void SetBools()
    {
        if (triggerOnLoad)
            isInCountdown = true;
        if (isInCountdown)
            canDisableHint = true;
        if (hasBeenCompleted)
            shouldDisplay = false;
    }

    public void TriggerHint()
    {
        canDisableHint = true;
        if (shouldDisplay && !hasBeenCompleted)
            isInCountdown = true;
    }

    public void DisableHint()
    {
        shouldDisplay = false;
        isInCountdown = false;
        hasBeenCompleted = true;
        UIHints.RemoveHint(hintData.hintName);
    }

    public void Display() {
        if (shouldDisplay && !hasBeenCompleted && !hasBeenAddedToDisplay)
        {
            UIHints.AddHint(hintData);
            hasBeenAddedToDisplay = true;
        }
    }
}