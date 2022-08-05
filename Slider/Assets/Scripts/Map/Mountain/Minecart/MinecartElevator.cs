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
        mc.SnapToRail(bottomPosition.transform.position, 3);
        mc.StartMoving();
    }

    public void SendMinecartUp(Minecart mc)
    {
        if(!isFixed)
            return;
        mc.StopMoving();
        mc.SnapToRail(topPosition.transform.position, 3);
        mc.StartMoving();
    }

    public void CheckIsFixed(Condition c)
    {
        c.SetSpec(isFixed);
    }
}
