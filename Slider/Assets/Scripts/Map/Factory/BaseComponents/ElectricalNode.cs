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

    protected void Awake()
    {
        powerPathPrevs = new HashSet<ElectricalNode>();
        powerRefs = 0;  //Always start off and let things turn on.
    }

    public virtual void StartSignal(bool input)
    {
        if (nodeType == NodeType.OUTPUT)
        {
            //According to the OOD gurus and lord Aibek, this violates Liskov Substitution (I think). Oh well.
            Debug.LogError("Cannot start signal from an OUTPUT Node");
        }

        if (Powered != input)    //This ensures we don't double propagate
        {
            powerRefs = input ? 1 : 0;

            foreach (ElectricalNode node in neighbors)
            {
                HashSet<ElectricalNode> recStack = new HashSet<ElectricalNode>() { this };
                node.PropagateSignal(input, this, recStack);
            }
        }
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
            powerRefs = Mathf.Max(powerRefs - numRefs, 0);
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
    //Returns the number of paths found
    public int GetPathNodes(out HashSet<ElectricalNode> nodes)
    {
        //Recursively get the path nodes by traversing the prev references made in propagation
        nodes = new HashSet<ElectricalNode>();

        return GetPathNodesRecursive(nodes);
    }

    private int GetPathNodesRecursive(HashSet<ElectricalNode> nodes)
    {
        nodes.Add(this);

        int refs = powerPathPrevs.Count == 0 ? 1 : 0;   //1 for the base case, 0 otherwise

        //Make sure the chain of prevs has no cycles (which is should) or else this will cause stack overflow
        foreach (ElectricalNode prev in powerPathPrevs)
        {
            refs += prev.GetPathNodesRecursive(nodes);
        }

        return refs;
    }

    //Could use the event here instead, but why do that?
    public virtual void OnPoweredHandler(bool value, bool valueChanged) { }

    //Target Complexity: O(n * p) p is the number of paths in this node as well as other (i.e. refs)
    //This method needs to not only add the neighbor, but update the state and ref counts to reflect the change (which can get more complicated).
    protected virtual void AddNeighbor(ElectricalNode other)
    {
        //The neighbor is already added, this prevents double counting.
        if (neighbors.Contains(other) || other.neighbors.Contains(this))
        {
            //Debug.Log($"{gameObject.name} already has an edge including {other.gameObject.name}. This method does nothing.");
            return;
        }

        if (this.nodeType == NodeType.IO && other.nodeType == NodeType.IO)
        {
            //Undirected case (Note: I'm using "this" mainly for readability to distinguish from other and show the parallelism)

            HashSet<ElectricalNode> otherNodes;
            other.GetPathNodes(out otherNodes);
            HashSet<ElectricalNode> thisNodes;
            this.GetPathNodes(out thisNodes);
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
                HashSet<ElectricalNode> thisNodes;
                this.GetPathNodes(out thisNodes);

                other.PropagateSignal(true, this, thisNodes, this.powerRefs);
            }
            this.neighbors.Add(other);
        } else if (this.nodeType != NodeType.INPUT && other.nodeType != NodeType.OUTPUT)
        {
            //Directed edge from other to this
            if (other.Powered)
            {
                HashSet<ElectricalNode> otherNodes;
                other.GetPathNodes(out otherNodes);

                this.PropagateSignal(true, other, otherNodes, other.powerRefs);
            }
            other.neighbors.Add(this);
        } else
        {
            //Any other cases are essentially not allowed.
            Debug.LogError("Attempted to create a connection going out of an output node or into an input node, this is not allowed.");
        }

    }

    //Target Complexity : O(n) p is the number of paths in this node as well as other (i.e. refs)
    protected virtual void RemoveNeighbor(ElectricalNode other)
    {
        //The neighbor is already removed, this prevents double counting.
        if (!neighbors.Contains(other) && !other.neighbors.Contains(this))
        {
            //Debug.Log($"{gameObject.name} does not have an edge including {other.gameObject.name}. This method does nothing.");
            return;
        }

        if (this.nodeType == NodeType.IO && other.nodeType == NodeType.IO)
        {
            //This goes in the opposite order as AddNeighbor, so we remove the edge first.
            this.neighbors.Remove(other);
            other.neighbors.Remove(this);

            bool powerPathToOther = this.powerPathPrevs.Remove(other);
            bool powerPathToThis = other.powerPathPrevs.Remove(this);

            //Undirected Case
            if (powerPathToOther)  //Remove checks if the set contains other
            {
                //This automatically means that other originally had an on state since it was included in a path (i.e. nonzero powerREfs)

                HashSet<ElectricalNode> otherNodes;
                int otherRefsNew = other.GetPathNodes(out otherNodes);
                otherNodes.Add(this);
                other.powerRefs = otherRefsNew;

                //Deletes ("Breaks") the paths that pass from other to this
                foreach (ElectricalNode neighbor in this.neighbors)
                {
                    neighbor.PropagateSignal(false, this, otherNodes, otherRefsNew);
                }
            }
            if (powerPathToThis)
            {
                HashSet<ElectricalNode> thisNodes;
                int thisRefsNew = this.GetPathNodes(out thisNodes);
                thisNodes.Add(other);
                this.powerRefs = thisRefsNew;

                foreach (ElectricalNode neighbor in other.neighbors)
                {
                    neighbor.PropagateSignal(false, other, thisNodes, thisRefsNew);
                }
            }
        }
        else if (this.nodeType != NodeType.OUTPUT && other.nodeType != NodeType.INPUT)
        {
            //Directed edge from this to other.
            this.neighbors.Remove(other);
            if (other.powerPathPrevs.Remove(this))
            {
                //Don't need to update this.powerRefs since it can't change.
                HashSet<ElectricalNode> thisNodes;
                this.GetPathNodes(out thisNodes);
                other.PropagateSignal(false, this, thisNodes, this.powerRefs);
            }
        }
        else if (this.nodeType != NodeType.INPUT && other.nodeType != NodeType.OUTPUT)
        {
            //Directed edge from other to this
            other.neighbors.Remove(this);
            if (this.powerPathPrevs.Remove(other))
            {
                //Don't need to update this.powerRefs since it can't change.
                HashSet<ElectricalNode> otherNodes;
                other.GetPathNodes(out otherNodes);
                this.PropagateSignal(false, other, otherNodes, other.powerRefs);
            }
        }
        else
        {
            //Any other cases are essentially not allowed.
            Debug.LogError("Attempted to create a connection going out of an output node or into an input node, this is not allowed.");
        }
    }
}