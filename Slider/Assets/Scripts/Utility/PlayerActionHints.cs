using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionHints : MonoBehaviour
{
    //C: Im trying to write this in a way that is as modular as possible. Good luck me.
    public List<Hint> hintsList;
    private List<Hint> hintQueue;
    private double currHintTimer = 0;
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
                    h.Display();
                }
            }
       }
    }

    public void TriggerHint(string hint)
    {
        foreach(Hint h in hintsList)
            if(string.Equals(h.hintName, hint))
            {
                hintQueue.Add(h);
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

    public bool TriggerHint()
    {
        if(timeUntilTrigger >= 0)
        {
            isInCountdown = true;
        }
        return timeUntilTrigger >= 0;
    }

    public void Display() {
        isInCountdown = false;
        if(shouldDisplay)
        {
            UIHints.AddHint(hintText);
        }
    }

    private void OnDisable()
    {

    }
}