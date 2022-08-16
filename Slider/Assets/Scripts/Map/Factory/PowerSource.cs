using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : ElectricalNode
{
    [Header("Power Source")]
    [SerializeField] SpriteSwapper swapper;
    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.INPUT;
    }

    void Start()
    {
        StartSignal(true);
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);

        if (swapper != null)
        {
            if (e.powered)
            {
                swapper?.TurnOn();
            }
            else
            {
                swapper?.TurnOff();
            }
        }
    }
}
