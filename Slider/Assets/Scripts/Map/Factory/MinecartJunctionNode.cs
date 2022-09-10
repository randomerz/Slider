using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinecartJunctionNode : ElectricalNode
{
    private Vector3Int loc;
    private RailManager rm;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;
        rm = GetComponentInParent<RailManager>();
        loc = rm.railMap.WorldToCell(transform.position);
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
        rm.ChangeTile(loc);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}
