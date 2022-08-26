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
    private bool isOpen; //C: true if there are tiles in front of the elevator (top and bottom), false otherwise



    public void FixElevator()
    {
        isFixed = true;
        mainMc.UpdateState("Empty");
        //update sprites/animate/whatnot
    }

    public void SendMinecartDown(Minecart mc)
    {
        if(!isFixed)
            return;
        mc.StopMoving();
        StartCoroutine(WaitThenSend(mc, bottomPosition.transform.position, 3));
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

    public void CheckIsValid(Condition c)
    {
        c.SetSpec(ValidElevator());
    }

    public bool ValidElevator()
    {
        return SGrid.Current.GetGrid()[0,1].isTileActive && SGrid.Current.GetGrid()[0,3].isTileActive;
    }

    private IEnumerator WaitThenSend(Minecart mc, Vector3 position, int dir){
        yield return new WaitForSeconds(3.0f);
        mc.SnapToRail(position, dir);
        mc.StartMoving();
    }
}
