using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinecartElevator : MonoBehaviour
{
    [SerializeField] private bool isFixed;
    public GameObject topPosition;
    public GameObject bottomPosition;
    public Minecart mainMc;
  //  public RailManager borderRM;
    private bool isOpen = true; //C: TODO true if there are tiles in front of the elevator (top and bottom), false otherwise
    private bool hasGoneDown;

    private void OnEnable() {
        SGridAnimator.OnSTileMoveEnd += CheckOpenOnMove;
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveEnd -= CheckOpenOnMove;
    }

    private void CheckOpenOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        isOpen = SGrid.Current.GetGrid()[0,1].isTileActive && SGrid.Current.GetGrid()[0,3].isTileActive;
        //set bool in animator
    }


    public void FixElevator()
    {
        isFixed = true;
        mainMc.UpdateState("Empty");
        AudioManager.Play("Puzzle Complete");
        //update sprites/animate/whatnot
    }

    public void SendMinecartDown(Minecart mc)
    {
        if(!isFixed)
            return;
        mc.StopMoving();
        StartCoroutine(WaitThenSend(mc, bottomPosition.transform.position, 3));
        hasGoneDown = true;
    }

    public void SendMinecartUp(Minecart mc)
    {
        if(!isFixed)
            return;
        mc.StopMoving();
        StartCoroutine(WaitThenSend(mc, topPosition.transform.position, 3));
    }

    public void CheckIsFixed(Condition c)
    {
        c.SetSpec(isFixed);
    }

    public void CheckIsNotOpen(Condition c)
    {
        c.SetSpec(!isOpen);
    }

    public void CheckHasGoneDown(Condition c)
    {
        c.SetSpec(hasGoneDown);
    }

    private IEnumerator WaitThenSend(Minecart mc, Vector3 position, int dir){
        yield return new WaitForSeconds(0.5f);
        mc.SnapToRail(position, dir);
        mc.StartMoving();
    }
}
