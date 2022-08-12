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

    private IEnumerator WaitThenSend(Minecart mc, Vector3 position, int dir){
        yield return new WaitForSeconds(3.0f);
        mc.SnapToRail(position, dir);
        mc.StartMoving();
    }
}
