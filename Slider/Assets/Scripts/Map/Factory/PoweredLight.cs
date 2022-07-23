using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredLight : ElectricalNode
{
    [SerializeField] protected SpriteSwapper swapper;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;

        if (swapper == null)
        {
            swapper = GetComponent<SpriteSwapper>();
        }
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
        if (e.powered)
        {
            swapper.TurnOn();
        } else
        {
            swapper.TurnOff();
        }
    }
}
