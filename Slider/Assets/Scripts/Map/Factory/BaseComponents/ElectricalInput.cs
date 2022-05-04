using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Note: ElectricalInput does not propagate since it is what initiates the propagation
public abstract class ElectricalInput : ElectricalNode
{
    [SerializeField]
    protected List<ElectricalNode> outputNodes;

    public sealed override void PropagateSignal(bool value, List<ElectricalNode> recStack, int numSignals = 1)
    {   //Can't propagate to an input.
    }
}
