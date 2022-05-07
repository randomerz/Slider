using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBox : ConductiveElectricalNode
{

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.INPUT;
    }

    void Start()
    {
        StartSignal(true);
    }
}
