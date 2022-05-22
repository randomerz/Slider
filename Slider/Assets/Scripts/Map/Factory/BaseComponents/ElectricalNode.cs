using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This is what controls the electrical system for the Factory/any other system that uses it.
public class ElectricalNode : MonoBehaviour
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

    //These are serialized for debugging purposes. They should not need to be set in the inspector.
    [SerializeField]
    protected int powerRefs;
    [SerializeField]
    protected HashSet<ElectricalNode> powerPathPrevs;  //This is used for backtracking paths to a power source.

    //NEIGHBORS ARE OUTGOING EDGES (or undirected)
    [SerializeField]
    protected List<ElectricalNode> neighbors;

    [SerializeField] protected bool invertSignal = false;

    public virtual bool Powered => invertSignal ? powerRefs <= 0 : powerRefs > 0; //This is marked virtual so we can have different powering conditions (see TimedGate.cs)

    public class OnPoweredArgs
    {
        public bool powered;
    }
    //public static event System.EventHandler<OnPoweredArgs> OnPowered;

    [SerializeField]
    public UnityEvent<OnPoweredArgs> OnPowered;

    protected void Awake()
    {
        powerPathPrevs = new HashSet<ElectricalNode>();
        powerRefs = 0;  //Always start off and let things turn on.
    }

    protected void OnEnable()
    {
        OnPowered.AddListener(OnPoweredHandler);
    }

    protected void OnDisable()
    {
        OnPowered.RemoveListener(OnPoweredHandler);
    }

    private void Start()
    {
        if (Powered)
        {
            //This is mainly for inverted power.
            OnPowered?.Invoke(new OnPoweredArgs { powered = Powered });
        }
    }

    public virtual void OnPoweredHandler(OnPoweredArgs e) { }

    public virtual void StartSignal(bool input)
    {
        if (nodeType != NodeType.INPUT)
        {
            //According to the OOD gurus and lord Aibek, this violates some design principle (I think). Oh well.
            Debug.LogError("Can only start a signal from an INPUT node.");
        }

        if (Powered != input)    //This ensures we don't double propagate
        {
            powerRefs = input ? 1 : 0;

            OnPowered?.Invoke(new OnPoweredArgs { powered = Powered });

            foreach (ElectricalNode node in neighbors)
            {
                HashSet<ElectricalNode> recStack = new HashSet<ElectricalNode>() { this };
                node.PropagateSignal(input, this, recStack);
            }
        }
    }

    //Target Complexity : O(n) including recursive calls (so updating node state should be O(1))
    protected virtual void PropagateSignal(bool value, ElectricalNode prev, HashSet<ElectricalNode> recStack, int numRefs = 1)
    {
        //These two methods are split in order to implement buffered input (which is only used for timed gates).
        bool oldPowered = Powered;
        if (EvaluateNodeInput(value, prev, recStack, numRefs))
        {
            //Call the event/handlers (this should only be used for nodes to respond to inputs, NOT to update node state)
            if (Powered != oldPowered)
            {
                OnPowered?.Invoke(new OnPoweredArgs { powered = Powered });
            }

            PushSignalToOutput(value, recStack, numRefs);
        }

    }

    //Takes the signal in, updates the node's state (powerRefs, powerPathPrevs)
    protected bool EvaluateNodeInput(bool value, ElectricalNode prev, HashSet<ElectricalNode> recStack, int numRefs = 1)
    {
        if (nodeType == NodeType.INPUT)
        {
            Debug.LogError("Cannot propogate a signal through an INPUT Node Type.");
        }

        if (recStack.Contains(this))
        {  //Cycle Detection
            return false;
        }

        bool oldPowered = Powered;

        //Update the reference counter
        //An assumption is made here that value is a newly updated value, otherwise the refCount strategy does not work.
        if (value)
        {
            powerRefs = powerRefs + numRefs;
            powerPathPrevs.Add(prev);
        }
        else
        {
            powerRefs = Mathf.Max(powerRefs - numRefs, 0);
            powerPathPrevs.Remove(prev);
        }

        return true;
    }

    //Takes the existing signal on the node and pushes it to the node's neighbors.
    protected void PushSignalToOutput(bool value, HashSet<ElectricalNode> recStack, int numRefs = 1)
    {
        //Propagate new signal to all other neighbors
        recStack.Add(this);
        foreach (ElectricalNode neighbor in neighbors)
        {
            if (!recStack.Contains(neighbor))
            {
                neighbor.PropagateSignal(value, this, recStack, numRefs);
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

        int refs = powerPathPrevs.Count == 0 ? 1 : 0;   //If a node doesn't have a prev, then it is the origin of the signal.

        //Make sure the chain of prevs has no cycles (which is should) or else this will cause stack overflow
        foreach (ElectricalNode prev in powerPathPrevs)
        {
            if (!nodes.Contains(prev))  //Need to add this to prevent stack overflow w/ more than 1 power source.
            {
                refs += prev.GetPathNodesRecursive(nodes);
            }
        }

        return refs;
    }

    //Target Complexity: O(n * p) p is the number of paths in this node as well as other (i.e. refs)
    //This method needs to not only add the neighbor, but update the state and ref counts of other nodes to reflect the change (which can get more complicated).
    public virtual void AddNeighbor(ElectricalNode other)
    {
        if (other == null)
        {
            Debug.LogError("You cannot add a null neighbor to ElectricalNode");
            return;
        }
        //The neighbor is already added, this prevents double counting.
        if (neighbors.Contains(other) || other.neighbors.Contains(this))
        {
            //Debug.Log($"{gameObject.name} already has an edge including {other.gameObject.name}. This method does nothing.");
            return;
        }

        //Debug.Log($"Adding Node {other.gameObject} to node {this.gameObject}");

        if (this.nodeType == NodeType.IO && other.nodeType == NodeType.IO)
        {
            //Undirected case (Note: I'm using "this" mainly for readability to distinguish from other and show the parallelism)

            //We only propagate "On's" for adding since paths can only be formed, not broken.
            //Essentially, since we created a new edge, there are now new possible paths from a power source to nodes that are reachable from this to other.
            //In order to account for these new paths, we need to propagate "across the edge" in both directions, so other's refs are propagated to this's neighbors and vice-versa.
            //BOTH propagations happen BEFORE we update the nodes themselves in order to avoid double counting refs.
            int oldThisRefs = this.powerRefs;   //If we don't do this, and power is coming from both directions, it will double count refs.
            bool oldPowered = this.Powered;
            if (other.Powered)
            {
                HashSet<ElectricalNode> otherNodes;
                other.GetPathNodes(out otherNodes);
                this.PropagateSignal(true, other, otherNodes, other.powerRefs);
            }
            if (oldPowered)
            {
                HashSet<ElectricalNode> thisNodes;
                this.GetPathNodes(out thisNodes);
                other.PropagateSignal(true, this, thisNodes, oldThisRefs);
            }

            //Create an undirected edge between the two
            this.neighbors.Add(other);
            other.neighbors.Add(this);
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
    public virtual void RemoveNeighbor(ElectricalNode other)
    {
        //The neighbor is already removed, this prevents double counting.
        if (!neighbors.Contains(other) && !other.neighbors.Contains(this))
        {
            //Debug.Log($"{gameObject.name} does not have an edge including {other.gameObject.name}. This method does nothing.");
            return;
        }

        //Debug.Log($"Removing Node {other.gameObject} from node {this.gameObject}");

        if (this.nodeType == NodeType.IO && other.nodeType == NodeType.IO)
        {
            //Undirected Case

            //The procedure goes in the opposite order as AddNeighbor, so we remove the edge first.
            this.neighbors.Remove(other);
            other.neighbors.Remove(this);

            bool powerPathFromOtherToThis = this.powerPathPrevs.Remove(other);  //false
            bool powerPathFromThisToOther = other.powerPathPrevs.Remove(this);   //true


            if (powerPathFromOtherToThis)
            {
                HashSet<ElectricalNode> otherNodes;
                int otherRefsNew = other.GetPathNodes(out otherNodes);
                other.powerRefs = otherRefsNew;

                //Deletes ("Breaks") the paths that pass from other to this
                this.PropagateSignal(false, other, otherNodes, otherRefsNew);
            }
            if (powerPathFromThisToOther)
            {
                HashSet<ElectricalNode> thisNodes;
                int thisRefsNew = this.GetPathNodes(out thisNodes);
                this.powerRefs = thisRefsNew;

                other.PropagateSignal(false, this, thisNodes, thisRefsNew);
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
            Debug.LogError("Attempted to remove a connection going out of an output node or into an input node, this is not allowed.");
        }
    }
}