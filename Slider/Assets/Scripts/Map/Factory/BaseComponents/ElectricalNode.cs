using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This was originally an interface, but I want to serialize it, so can't do that.
public abstract class ElectricalNode : MonoBehaviour
{
    public abstract void PropagateSignal(bool value, List<ElectricalNode> recStack, int numSignals = 1);
}
