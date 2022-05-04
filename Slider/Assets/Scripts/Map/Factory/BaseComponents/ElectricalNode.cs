using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This was originally an interface, but I want to serialize it, so can't do that.
public abstract class ElectricalNode : MonoBehaviour
{

    public enum NodeType
    {
        IO,
        INPUT, 
        OUTPUT,
    }

    public NodeType nodeType;

    protected int powerRefCount;

    [SerializeField]
    protected List<ElectricalNode> neighbors;

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
        public bool valueChanged;
    }
    public static event System.EventHandler<OnPoweredArgs> OnPowered;

    private void Awake()
    {
        powerRefCount = 0;
    }

    public virtual void PropagateSignal(bool value, List<ElectricalNode> recStack, int numSignals = 1)
    {
        if (nodeType == NodeType.INPUT)
        {
            Debug.LogError("Cannot propogate a signal through an INPUT Node Type.");
        }

        //Update the reference counter
        //An assumption is made here that value is a newly updated value, otherwise the refCount strategy does not work.
        bool oldPowered = Powered;
        powerRefCount = value ? powerRefCount + numSignals : Mathf.Max(powerRefCount - numSignals, 0);

        //Call the event/handlers
        bool valueChanged = Powered != oldPowered;
        OnPoweredHandler(value, Powered != oldPowered);
        OnPowered?.Invoke(this, new OnPoweredArgs { powered = Powered, valueChanged = valueChanged });

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

    //Could use the event here instead, but why do that?
    public virtual void OnPoweredHandler(bool value, bool valueChanged)
    {

    }

    protected virtual void AddNeighbor(ElectricalNode other, bool directed = false)
    {
        //This method is only responsible for updating its own neighbors list with other.
        if (this.nodeType != NodeType.OUTPUT && other.nodeType != NodeType.INPUT)   //Can't go out of an output node, and can't go into an input node.
        {
            neighbors.Add(other);
        }
    }
}
