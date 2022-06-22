using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionHints : MonoBehaviour
{
    //C: Im trying to write this in a way that is as modular as possible. Good luck me.
    public List<Hint> hintsList;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerHint(string hint)
    {
        foreach(Hint h in hintsList)
            if(string.Equals(h.hintName, hint))
                h.TriggerHint();
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
    public string hintName {get; private set;} //used when searching through hints
    private string hintText; //the text of the hint
    private double timeUntilTrigger; //how long from triggering the hint until it displays
    private bool isInCountdown = false; //is this hint counting down until display?
    public bool shouldDisplay {get; set;} = true; //should this hint display?

    public void TriggerHint()
    {
        isInCountdown = true;
    }

    private void Update()
    {
        if(isInCountdown && !(UIManager.GetInstance().isGamePaused))
        {
            timeUntilTrigger -= Time.deltaTime;
            if(timeUntilTrigger < 0)
            {
                isInCountdown = false;
                if(shouldDisplay)
                {
                    UIHints.AddHint(hintText);
                }
            }
        }
    }

    private void OnDisable()
    {

    }
}