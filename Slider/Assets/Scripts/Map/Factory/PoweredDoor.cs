using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredDoor : ElectricalNode
{
    //Probably want to do an animation later instead of sprite swapping

    [SerializeField]
    private GameObject off;
    [SerializeField]
    private GameObject on;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        off.SetActive(!e.powered);
        on.SetActive(e.powered);
    }
}
