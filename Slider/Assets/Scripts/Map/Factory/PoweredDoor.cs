using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredDoor : ElectricalNode
{
    //Probably want to do an animation later instead of sprite swapping

    [SerializeField]
    GameObject off;
    [SerializeField]
    GameObject on;

    public void OnPoweredHandler(OnPoweredArgs e)
    {
        Debug.Log("Yes?");
        off.SetActive(!e.powered);
        on.SetActive(e.powered);
    }
}
