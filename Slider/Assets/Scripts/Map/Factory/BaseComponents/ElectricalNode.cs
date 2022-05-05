using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This was originally an interface, but I want to serialize it, so can't do that.
public abstract class ElectricalNode : MonoBehaviour
{

    public enum NodeType
    {
        //These determine what type of edges in the graph there are.
        IO,
        INPUT,
        OUTPUT,

        //IO to IO is undirected
        //Directed edges go from INPUT to IO, IO to OUTPUT, and INPUT to OUTPUT
    }

    public NodeType nodeType;

    protected int powerRefs;
    protected HashSet<ElectricalNode> powerPathPrevs;

    [SerializeField]
    protected List<ElectricalNode> neighbors;

    public bool Powered
    {
        get
        {
            return powerRefs > 0;
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
        powerPathPrevs = new HashSet<ElectricalNode>();
        powerRefs = 0;
    }

    //Target Complexity : O(n) including recursive calls (so updating node state should be O(1)
    public virtual void PropagateSignal(bool value, ElectricalNode prev, HashSet<ElectricalNode> recStack, int numRefs = 1)
    {
        if (nodeType == NodeType.INPUT)
        {
            Debug.LogError("Cannot propogate a signal through an INPUT Node Type.");
        }

        if (recStack.Contains(this)) {  //Cycle Detection
            return;
        }

        bool oldPowered = Powered;

        //Update the reference counter
        //An assumption is made here that value is a newly updated value, otherwise the refCount strategy does not work.
        if (value)
        {
            powerRefs = powerRefs + numRefs;
            powerPathPrevs.Add(prev);
        } else
        {
            powerRefs = powerRefs - numRefs;
            powerPathPrevs.Remove(prev);
        }

        /*
        if (powerRefTable.ContainsKey(prev))
        {
            powerRefTable[prev] = value ? powerRefTable[prev] + numSignals : Mathf.Max(powerRefTable[prev] - numSignals, 0);
        } else
        {
            powerRefTable[prev] = value ? numSignals : 0;
        }
        */

        //Call the event/handlers (this should only be used for devices to respond to things, NOT to update node state)
        bool valueChanged = Powered != oldPowered;
        OnPoweredHandler(value, Powered != oldPowered);
        OnPowered?.Invoke(this, new OnPoweredArgs { powered = Powered, valueChanged = valueChanged });

        //Propagate new signal to all other neighbors
        recStack.Add(this);
        foreach (ElectricalNode neighbor in neighbors)
        {
            if (!recStack.Contains(neighbor))
            {
                neighbor.PropagateSignal(value, this, recStack);
            }
        }
        recStack.Remove(this);
    }

    //Target Complexity : O(n)
    //Note: this isn't cached from propagate because we only want to get the path when necessary (not for every node on every update)
    public HashSet<ElectricalNode> GetPathNodes(bool includeThis = true)
    {
        HashSet<ElectricalNode> output = new HashSet<ElectricalNode>();
        //Recursively get the path nodes by traversing the prev references made in propagation

        if (includeThis) {
            output.Add(this);
        }
        foreach (ElectricalNode prev in powerPathPrevs)
        {
            output.UnionWith(prev.GetPathNodes(true));
        }

        return output;
    }

    //Could use the event here instead, but why do that?
    public virtual void OnPoweredHandler(bool value, bool valueChanged) { }

    //Target Complexity: O(n * p) p is the number of paths in this node as well as other (i.e. ref counts)
    //This method needs to not only add the neighbor, but update the state and ref counts to reflect the change (which can get more complicated).
    protected virtual void AddNeighbor(ElectricalNode other)
    {
        //The neighbor is already added, this prevents double counting.
        if (neighbors.Contains(other) || other.neighbors.Contains(this))
        {
            return;
        }

        if (this.nodeType == NodeType.IO && other.nodeType == NodeType.IO)
        {
            //Undirected case (Note: I'm using "this" mainly for readability to distinguish from other and show the parallelism)

            HashSet<ElectricalNode> otherNodes = other.GetPathNodes();
            HashSet<ElectricalNode> thisNodes = this.GetPathNodes();
            otherNodes.UnionWith(new HashSet<ElectricalNode>() { this });
            thisNodes.UnionWith(new HashSet<ElectricalNode>() { other });

            //We only propagate "On's" for adding since paths can only be formed, not broken.
            //Essentially, since we created a new edge, there are now new possible paths from a power source to nodes that are reachable from this to other.
            //In order to account for these new paths, we need to propagate "across the edge" in both directions, so other's refs are propagated to this's neighbors and vice-versa.
            //BOTH propagations happen BEFORE we update the nodes themselves in order to avoid double counting refs.
            if (other.Powered)
            {

                foreach (ElectricalNode neighbor in this.neighbors)
                {
                    neighbor.PropagateSignal(true, this, otherNodes, other.powerRefs);
                }
                this.powerPathPrevs.Add(other);
            }
            if (this.Powered)
            {
                foreach (ElectricalNode neighbor in other.neighbors)
                {
                    neighbor.PropagateSignal(true, other, thisNodes, this.powerRefs);
                }
                other.powerPathPrevs.Add(this);
            }

            //Create an undirected edge between the two
            this.neighbors.Add(other);
            other.neighbors.Add(this);

            //Update the state of this and other.
            int totalRefCount = this.powerRefs + other.powerRefs;
            this.powerRefs = totalRefCount;
            other.powerRefs = totalRefCount; 
        } else if (this.nodeType != NodeType.OUTPUT && other.nodeType != NodeType.INPUT)
        {
            //Directed edge from this to other.
            if (this.Powered)
            {
                HashSet<ElectricalNode> thisNodes = this.GetPathNodes();
                other.PropagateSignal(true, this, thisNodes, this.powerRefs);
                this.neighbors.Add(other);
            }
        } else if (this.nodeType != NodeType.INPUT && other.nodeType != NodeType.OUTPUT)
        {
            //Directed edge from other to this
            if (other.Powered)
            {
                HashSet<ElectricalNode> otherNodes = this.GetPathNodes();
                this.PropagateSignal(true, other, otherNodes, other.powerRefs);
                other.neighbors.Add(this);
            }
        } else
        {
            //Any other cases are essentially not allowed.
            Debug.LogError("Attempted to create a connection going out of an output node or into an input node, this is not allowed.");
        }

    }

    //Target Complexity : O(n * p)
    protected virtual void RemoveNeighbor(ElectricalNode other)
    {
        
    }
}
