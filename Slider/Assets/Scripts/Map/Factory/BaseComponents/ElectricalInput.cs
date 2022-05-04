using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricalInput : ElectricalNode
{
    public override void PropagateSignal(bool value, List<ElectricalNode> recStack, int numSignals = 1)
    {
        Debug.LogError("Tried to propagate forward a signal from an input node.");
    }
    protected override void AddNeighbor(ElectricalNode other, bool directed = false)
    {
        neighbors.Add(other);
    }
}
