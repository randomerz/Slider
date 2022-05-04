using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Electrical Component that always propagates its value 
public class ElectricalConductor : ElectricalNode
{
    private int powerRefCount;

    [SerializeField]
    private List<ElectricalNode> neighbors;

    public bool Powered
    {
        get
        {
            return powerRefCount > 0;
        }
    }

    public class OnPoweredArgs
    {
        public bool powered;
    }
    public static event System.EventHandler<OnPoweredArgs> OnPowered;

    private void Awake()
    {
        powerRefCount = 0;
    }

    public override void PropagateSignal(bool value, List<ElectricalNode> recStack, int numSignals = 1)
    {
        //An assumption is made here that value is a newly updated value, otherwise the refCount strategy does not work.
        bool oldPowered = Powered;
        powerRefCount = value ? powerRefCount + numSignals : Mathf.Max(powerRefCount - numSignals, 0);

        if (Powered != oldPowered)
        {
            OnPoweredHandler(value);
            OnPowered?.Invoke(this, new OnPoweredArgs { powered = Powered });
        }

        //Propagate new signal to all other neighbors
        recStack.Add(this);
        foreach (ElectricalNode neighbor in neighbors)
        {
            if (!recStack.Contains(neighbor))
            {
                neighbor.PropagateSignal(value, recStack);
            }
        }

        recStack.Remove(this);
    }

    public virtual void OnPoweredHandler(bool value)
    {

    }
}
