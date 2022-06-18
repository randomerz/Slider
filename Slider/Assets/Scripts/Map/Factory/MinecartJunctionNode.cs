using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinecartJunctionNode : ElectricalNode
{
    [SerializeField] private Vector3Int loc;
    private RailManager rm;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;
        rm = GetComponentInParent<RailManager>();
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
        rm.ChangeTile(loc);
    }
}
