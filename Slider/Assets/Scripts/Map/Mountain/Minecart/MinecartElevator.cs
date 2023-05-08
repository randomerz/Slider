using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinecartElevator : MonoBehaviour, ISavable
{
    [SerializeField] private bool isFixed;
    public GameObject topPosition;
    public GameObject bottomPosition;
    public Minecart mainMc;
  //  public RailManager borderRM;
    private bool isOpen = true; //C: TODO true if there are tiles in front of the elevator (top and bottom), false otherwise
    private bool hasGoneDown;
    public ElevatorAnimationManager animationManager;
    public bool isSending = false; //true when minecart being sent;

    private void OnEnable() {
        SGridAnimator.OnSTileMoveEnd += CheckOpenOnMove;
        SGridAnimator.OnSTileMoveStart += CheckOpenOnMove;
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveEnd -= CheckOpenOnMove;
        SGridAnimator.OnSTileMoveStart -= CheckOpenOnMove;
    }

    private void CheckOpenOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        isOpen = CheckIfShouldBeOpen();
    }


    public void FixElevator()
    {
        isFixed = true;
        mainMc.UpdateState("Empty");
        AudioManager.Play("Puzzle Complete");
        animationManager.Repair();
        //update sprites/animate/whatnot
    }

    public void SendMinecartDown(Minecart mc)
    {
        if(!isFixed)
            return;
        mc.StopMoving();
        animationManager.SendDown();
        StartCoroutine(WaitThenSend(mc, bottomPosition.transform.position, 3));
        hasGoneDown = true;
    }

    public void SendMinecartUp(Minecart mc)
    {
        if(!isFixed)
            return;
        mc.StopMoving();
        animationManager.SendUp();
        StartCoroutine(WaitThenSend(mc, topPosition.transform.position, 3));
    }
    
    private IEnumerator WaitThenSend(Minecart mc, Vector3 position, int dir){
        isSending = true;
        yield return new WaitForSeconds(2f);
        mc.SnapToRail(position, dir);
        yield return new WaitForSeconds(1.5f);
        mc.StartMoving();
        isSending = false;
    }

    public bool CheckIfShouldBeOpen()
    {
        Condition c = new Condition();
        c.type = Condition.ConditionType.gridStationary;
        return SGrid.Current.GetStileAt(0, 1).isTileActive && !SGrid.Current.GetStileAt(0, 1).IsMoving() 
        && SGrid.Current.GetStileAt(0, 3).isTileActive && !SGrid.Current.GetStileAt(0, 3).IsMoving();
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


    public void Save()
    {
        SaveSystem.Current.SetBool("MountainElevatorFixed", isFixed);
    }

    public void Load(SaveProfile profile)
    {
        isFixed = profile.GetBool("MountainElevatorFixed");
        if(isFixed)
            animationManager.Repair(true);
    }
}
