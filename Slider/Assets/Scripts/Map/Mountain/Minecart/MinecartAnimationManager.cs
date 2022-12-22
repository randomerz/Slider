using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinecartAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator mcAnimator;
    [SerializeField] private Animator contentsAnimator;
    [SerializeField] private string currentState;

    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        mcAnimator.Play(currentState);
        contentsAnimator.Play(currentState);
        currentState = newState;
    }

    public void ChangeAnimationState(int stateNum)
    {
        string stateName = ((MCAnimationState)stateNum).ToString();
        ChangeAnimationState(stateName);
    }

    public void ChangeContents()
    {

    }

    public void AnimateCorner(int currDir, int nextDir)
    {

    }
}


//C: Direction of travel or turn
public enum MCAnimationState 
{
    EAST,
    NORTH,
    WEST,
    SOUTH,
    ENTURN,
    ESTURN,
    NETURN,
    NWTURN,
    WNTURN,
    WSTURN,
    SETURN,
    SWTURN,
    STOPPEDEW,
    STOPPEDNS
}