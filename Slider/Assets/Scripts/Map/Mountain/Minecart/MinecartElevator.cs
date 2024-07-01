using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinecartElevator : MonoBehaviour, ISavable
{

    public GameObject topPosition;
    public GameObject bottomPosition;
    public Minecart mainMc;
    public bool hasGoneDown;
    public bool hasGoneUp;
    public ElevatorAnimationManager animationManager;
    public bool isSending = false; //true when minecart being sent;

    public List<SpriteSwapper> powerSwappers;

    public bool isInBreakingAnimation = false;
    public GameObject crystalchecker;
    public ElectricalNode powerBox;
    public GameObject pylon;

    public bool anchorGeneratorPower;

    public enum ElevatorState {
        INITIAL,
        BROKEN,
        FIXED
    }

    public ElevatorState elevatorState = ElevatorState.INITIAL;

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
        animationManager.Break(fromSave);
        powerBox.StartSignal(false);
        TogglePowerSprites(false);
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
        powerBox.StartSignal(true);
        TogglePowerSprites(true);

        if(!fromSave)
        {
            mainMc.UpdateState(MinecartState.Empty);
            AudioManager.Play("Puzzle Complete");
        }
    }

    public void SendMinecartDown(Minecart mc)
    {
        if(elevatorState == ElevatorState.BROKEN) return;
        mc.StopMoving();
        animationManager.SendDown();
        StartCoroutine(WaitThenSend(mc, bottomPosition.transform.position, 3, true));
    }

    public void SendMinecartUp(Minecart mc)
    {
        if(elevatorState == ElevatorState.BROKEN) return;
        mc.StopMoving();
        animationManager.SendUp();
        StartCoroutine(WaitThenSend(mc, topPosition.transform.position, 3));
        hasGoneUp = true;
    }
    
    private IEnumerator WaitThenSend(Minecart mc, Vector3 position, int dir, bool down = false){
        isSending = true;
        yield return new WaitForSeconds(2f);
        mc.SnapToRail(position, dir);
        if(down && !hasGoneDown)
        {
            hasGoneDown = true;
            AudioManager.Play("Puzzle Complete");
        }
        yield return new WaitForSeconds(1.5f);
        mc.StartMoving();
        isSending = false;
    }

    public void TogglePowerSprites(bool val)
    {
        foreach(SpriteSwapper ss in powerSwappers)
        {
            if(val)
                ss.TurnOn();
            else
                ss.TurnOff();
        }
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
            case ElevatorState.INITIAL:
                TogglePowerSprites(true);
                break;
        }
    }
}
