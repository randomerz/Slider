using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinecartElevator : MonoBehaviour
{
    [SerializeField] private bool isFixed;
    public GameObject topPosition;
    public GameObject bottomPosition;
  //  public RailManager borderRM;



    public void FixElevator()
    {
        isFixed = true;
        //update sprites/animate/whatnot
    }

    public void SendMinecartDown(Minecart mc)
    {
        if(!isFixed)
            return;
        mc.StopMoving();
        mc.SnapToRail(bottomPosition.transform.position, 3);
    }

    public void SendMinecartUp(Minecart mc)
    {
        if(!isFixed)
            return;
        mc.StopMoving();
        mc.SnapToRail(topPosition.transform.position, 3);
    }

    public void CheckIsFixed(Condition c)
    {
        c.SetSpec(isFixed);
    }
}
