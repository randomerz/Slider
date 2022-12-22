using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinecartAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator mcAnimator;
    private Animator contentsAnimator;
    private GameObject contentsSprite;
    [SerializeField] private Animator crystalAnimator;
    [SerializeField] private GameObject crystalSprite;
    [SerializeField] private Animator repairPartsAnimator;
    [SerializeField] private GameObject repairPartsSprite;
    [SerializeField] private Animator lavaAnimator;
    [SerializeField] private GameObject lavaSprite;

    


    [SerializeField] private string currentState; //animation state
    [SerializeField] private MinecartState mcState;

    private List<GameObject> objects = new List<GameObject>();
    //private List<Animator> animators = new List<Animator>();

    private void Awake() 
    {
        objects.Add(crystalSprite);
        objects.Add(repairPartsSprite);
        objects.Add(lavaSprite);
        //animators.Add(crystalAnimator);
        //animators.Add(repairPartsAnimator);
        //animators.Add(lavaAnimator);    
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        mcAnimator.Play(currentState);
        contentsAnimator?.Play(currentState);
        currentState = newState;
    }

    public void ChangeAnimationState(int stateNum)
    {
        string stateName = ((MCAnimationState)stateNum).ToString();
        ChangeAnimationState(stateName);
    }

    public void ChangeContents(MinecartState contents)
    {
        if(contents == mcState) return;
        switch(contents)
        {
            case MinecartState.Crystal:
                contentsAnimator = crystalAnimator;
                contentsSprite = crystalSprite;
                break;
            case MinecartState.Lava:
                contentsAnimator = lavaAnimator;
                contentsSprite = lavaSprite;
                break;
            case MinecartState.RepairParts:
                contentsAnimator = repairPartsAnimator;
                contentsSprite = repairPartsSprite;
                break;
            case MinecartState.Empty:
            default:
                contentsAnimator = null;
                contentsSprite = null;
                break;
        }

        mcState = contents;

        foreach(GameObject sprite in objects)
            sprite.SetActive(contentsSprite != null && contentsSprite == sprite);

        contentsAnimator?.Play(currentState);
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