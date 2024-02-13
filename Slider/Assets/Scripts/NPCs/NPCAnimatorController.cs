using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAnimatorController : MonoBehaviour
{
    public Animator myAnimator;

    private readonly string[] DEFAULT_ANIMATION_STATE_NAMES = {
        "Idle",
        "Walk",
    };
    private string currentState = ""; // not including default state names

    
    public void SetBoolToTrue(string str)
    {
        myAnimator.SetBool(str, true);
    }

    public void SetBoolToFalse(string str)
    {
        myAnimator.SetBool(str, false);
    }

    public void Play(string stateName)
    {
        if (!myAnimator.gameObject.activeInHierarchy)
            return;
            
        if (stateName == "" && currentState == "")
            return;
        
        if (stateName == "" && (currentState != "" || currentState == "Idle"))
        {
            currentState = "";
            myAnimator.Play("Idle");
            return;
        }

        if (stateName == currentState)
            return;
        currentState = stateName;

        myAnimator.Play(stateName);
    }

    public bool HasExtraAnimationStates()
    {
        return true;
    }
}
