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

    private void Awake() 
    {
        objects.Add(crystalSprite);
        objects.Add(repairPartsSprite);
        objects.Add(lavaSprite);  
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        mcAnimator.Play(currentState);
//        contentsAnimator?.Play(currentState);
        
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

       // foreach(GameObject sprite in objects)
          //  sprite.SetActive(contentsSprite != null && contentsSprite == sprite);

//        contentsAnimator?.Play(currentState);
    }

    public void SetSpeed(int speed)
    {
        mcAnimator.speed = speed;
       //crystalAnimator.speed = speed;
      //  lavaAnimator.speed = speed;
      //  repairPartsAnimator.speed = speed;
    }
}


//C: Direction of travel or turn
public enum MCAnimationState 
{
    IDLE,
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
}