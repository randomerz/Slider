using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : ConductiveElectricalNode
{
    [SerializeField]
    private GameObject off;
    [SerializeField]
    private GameObject on;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.IO;

        //Just initializing to make sure they're not both active at the same time
        off.SetActive(!Powered);
        on.SetActive(Powered);
    }

    public override void OnPoweredHandler(OnPoweredArgs args)
    {
        off.SetActive(!args.powered);
        on.SetActive(args.powered);
    }
}
