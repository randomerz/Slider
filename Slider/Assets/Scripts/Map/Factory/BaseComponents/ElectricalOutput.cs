using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ElectricalOutput : ElectricalNode
{
    [SerializeField]
    protected List<ElectricalNode> inputNodes;
}
