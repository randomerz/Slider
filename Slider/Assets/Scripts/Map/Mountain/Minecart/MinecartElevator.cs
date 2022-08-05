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
        mc.SnapToRailElevator(bottomPosition.transform.position);
    }

    public void SendMinecartUp(Minecart mc)
    {
        if(!isFixed)
            return;
        mc.StopMoving();
        mc.SnapToRailElevator(topPosition.transform.position);
    }
}
