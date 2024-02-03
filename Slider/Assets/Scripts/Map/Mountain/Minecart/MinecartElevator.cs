using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinecartElevator : MonoBehaviour, ISavable
{
    // [SerializeField] private bool isFixed;
    // [SerializeField] private bool isBroken;
    public GameObject topPosition;
    public GameObject bottomPosition;
    public Minecart mainMc;
  //  public RailManager borderRM;
   // private bool isOpen = true; //C: TODO true if there are tiles in front of the elevator (top and bottom), false otherwise
    private bool hasGoneDown;
    private bool hasGoneUp;
    public ElevatorAnimationManager animationManager;
    public bool isSending = false; //true when minecart being sent;

    public bool isInBreakingAnimation = false;
    public GameObject crystalchecker;
    public GameObject pylon;

    public bool anchorGeneratorPower;

    public enum ElevatorState {
        INTIAL,
        BROKEN,
        FIXED
    }

    public ElevatorState elevatorState = ElevatorState.INTIAL;

    // private void OnEnable() {
    //     SGridAnimator.OnSTileMoveEnd += CheckOpenOnMove;
    //     SGridAnimator.OnSTileMoveStart += CheckOpenOnMove;
    // }

    // private void OnDisable() {
    //     SGridAnimator.OnSTileMoveEnd -= CheckOpenOnMove;
    //     SGridAnimator.OnSTileMoveStart -= CheckOpenOnMove;
    // }

    // private void CheckOpenOnMove(object sender, SGridAnimator.OnTileMoveArgs e)
    // {
    //     isOpen = CheckIfShouldBeOpen();
    // }

    public void SetIsPowered(bool powered)
    {
        anchorGeneratorPower = powered;
    }

    public void BreakElevator()
    {
        BreakElevator(false);
    }

    public void BreakElevator(bool fromSave = false)
    {
        elevatorState = ElevatorState.BROKEN;
        crystalchecker.SetActive(true);
        isInBreakingAnimation = true;
        animationManager.Break();
    }

    public void FixElevator()
    {
        FixElevator(false);
    }

    public void FixElevator(bool fromSave = false)
    {
        if(elevatorState != ElevatorState.BROKEN && !fromSave)
            Debug.LogWarning("Fixed elevator when not in broken state");
        
        elevatorState = ElevatorState.FIXED;
        crystalchecker.SetActive(false);
        animationManager.Repair();
        pylon.SetActive(false);

        if(!fromSave)
        {
            mainMc.UpdateState("Empty");
            AudioManager.Play("Puzzle Complete");
        }
    }

    public void SendMinecartDown(Minecart mc)
    {
        if(elevatorState == ElevatorState.BROKEN) return;
        mc.StopMoving();
        animationManager.SendDown();
        StartCoroutine(WaitThenSend(mc, bottomPosition.transform.position, 3));
        hasGoneDown = true;
    }

    public void SendMinecartUp(Minecart mc)
    {
        if(elevatorState == ElevatorState.BROKEN) return;
        mc.StopMoving();
        animationManager.SendUp();
        StartCoroutine(WaitThenSend(mc, topPosition.transform.position, 3));
        hasGoneUp = true;
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

    public void CheckIsBroken(Condition c) => c.SetSpec(!isInBreakingAnimation && elevatorState == ElevatorState.BROKEN);

    public void CheckIsFixed(Condition c) => c.SetSpec(elevatorState == ElevatorState.FIXED);

    //public void CheckIsNotOpen(Condition c) => c.SetSpec(!isOpen);

    public void CheckHasGoneDown(Condition c) => c.SetSpec(hasGoneDown);

    public void CheckHasNotGoneDown(Condition c) => c.SetSpec(!hasGoneDown);

    public void CheckHasGoneUp(Condition c) => c.SetSpec(hasGoneUp);
    
    public void CheckGeneratorPoweringAnchor(Condition c) => c.SetSpec(anchorGeneratorPower);


    public void Save()
    {
        SaveSystem.Current.SetInt("MountainElevatorState", (int)elevatorState);
        SaveSystem.Current.SetBool("MountainElevatorUp", hasGoneUp);
        SaveSystem.Current.SetBool("MountainElevatorDown", hasGoneDown);
    }

    public void Load(SaveProfile profile)
    {
        hasGoneUp = profile.GetBool("MountainElevatorUp");
        hasGoneDown = profile.GetBool("MountainElevatorDown");
        elevatorState = (ElevatorState)profile.GetInt("MountainElevatorState");
        switch (elevatorState)
        {
            case ElevatorState.FIXED:
                FixElevator(true);
                break;
            case ElevatorState.BROKEN:
                BreakElevator(true);
                break;
        }
    }
}
