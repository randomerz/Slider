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

    [Header("Electrical Node")]
    [SerializeField, Tooltip("Determines which ways are possible to conduct.")] protected NodeType nodeType;

    [SerializeField, Tooltip("NEIGHBORS ARE OUTGOING EDGES")] protected List<ElectricalNode> neighbors;
    [SerializeField] protected bool powerOnStart;
    [SerializeField] protected bool invertSignal = false;
    [SerializeField] protected bool affectedByBlackout = true;

    protected HashSet<ElectricalNode> _outgoingNodes = new HashSet<ElectricalNode>();
    protected HashSet<ElectricalNode> _incomingNodes = new HashSet<ElectricalNode>();

    protected bool _isPowerSource = false;
    protected bool _isPowered = false;
    private bool _lastPoweredState = false;

    public class OnPoweredArgs
    {
        public bool powered;
    }
    public UnityEvent<OnPoweredArgs> OnPowered;
    public UnityEvent OnPoweredOn;
    public UnityEvent OnPoweredOff;

    public bool Powered => !FactoryBlackoutInEffect() && PoweredConditionsMet();
    public void PoweredSpec(Condition c) => c.SetSpec(Powered);

    protected virtual void Awake()
    {
        _outgoingNodes.UnionWith(neighbors);
        foreach (ElectricalNode node in neighbors)
        {
            node._incomingNodes.Add(this);
        }
    }

    protected virtual void OnEnable()
    {
        OnPowered.AddListener(OnPoweredHandler);
    }

    protected virtual void OnDisable()
    {
        OnPowered.RemoveListener(OnPoweredHandler);
    }

    private void OnDestroy()
    {
        RemoveAllConnections();
    }

    private void Start()
    {
        if (powerOnStart)
        {
            StartSignal(true);
        } else if (Powered)
        {
            OnPowered?.Invoke(new OnPoweredArgs { powered = Powered });
            _lastPoweredState = true;
        }
    }

    protected virtual void Update()
    {
        if (Powered != _lastPoweredState)
        {
            OnPowered?.Invoke(new OnPoweredArgs { powered = Powered });
        }
        _lastPoweredState = Powered;
    }

    protected virtual bool PoweredConditionsMet()
    {
        return (invertSignal ? !_isPowered : _isPowered);
    }

    protected bool FactoryBlackoutInEffect()
    {
        if(SGrid.Current == null) return false;
        bool inFactoryDuringBlackout = SGrid.Current.GetArea() == Area.Factory && PowerCrystal.Blackout;
        bool isAffected = affectedByBlackout && !FactoryGrid.IsInPast(gameObject);
        return inFactoryDuringBlackout && isAffected;
    }

    public virtual void OnPoweredHandler(OnPoweredArgs e) { 
        if (e.powered)
        {
            OnPoweredOn?.Invoke();
        } else
        {
            OnPoweredOff?.Invoke();
        }
    }

    public void StartSignal(bool value)
    {
        _isPowerSource = value;
        UpdateDFS();
    }

    protected bool IsConnectedToPower()
    {
        //DFS backwards (through incoming nodes) to find the power source
        var visited = new HashSet<ElectricalNode>();
        var stack = new Stack<ElectricalNode>();

        visited.Add(this);
        stack.Push(this);
        while (stack.Count > 0)
        {
            ElectricalNode curr = stack.Pop();

            if (curr._isPowerSource)
            {
                return true;
            }

            if (curr == this || curr.CanPropagateBackwards())   //Timed gate patch so we don't bypass them.
            {
                foreach (ElectricalNode node in curr._incomingNodes)
                {
                    if (node != null && !visited.Contains(node))
                    {
                        visited.Add(node);
                        stack.Push(node);
                    }
                }
            }
        }

        return false;
    }

    protected void UpdateDFS()
    {
        //DFS forwards (through outgoing nodes)
        var visited = new HashSet<ElectricalNode>();
        var stack = new Stack<KeyValuePair<ElectricalNode, ElectricalNode>>();

        visited.Add(this);
        stack.Push(new KeyValuePair<ElectricalNode, ElectricalNode>(this, null));
        while (stack.Count > 0)
        {
            var kv = stack.Pop();
            ElectricalNode curr = kv.Key;
            ElectricalNode prev = kv.Value;
            bool nodePowered = _isPowerSource ? true : curr.IsConnectedToPower();
            curr.EvaluateNodeDuringPropagate(nodePowered, prev);
            if (curr.CanPropagateForward(prev))
            {
                foreach (ElectricalNode node in curr._outgoingNodes)
                {
                    if (node != null && !visited.Contains(node))
                    {
                        visited.Add(node);
                        stack.Push(new KeyValuePair<ElectricalNode, ElectricalNode>(node, curr));
                    }
                }
            }
        }
    }

    //These are for timed gates rn bc of how they buffer signals

    protected virtual bool CanPropagateForward(ElectricalNode prev)   //arg used in overrides
    {
        return true;
    }

    protected virtual bool CanPropagateBackwards()
    {
        return true;
    }

    protected virtual void EvaluateNodeDuringPropagate(bool powered, ElectricalNode prev)
    {
        _isPowered = powered;
    }

    protected bool AddConnection(ElectricalNode other)
    {
        if (_outgoingNodes.Contains(other) || _incomingNodes.Contains(other))
        {
            return false;
        } 

        bool forwardDir = ValidDirectionGoingTo(other);
        if (forwardDir)
        {
            _outgoingNodes.Add(other);
            other._incomingNodes.Add(this);

            UpdateDFS();
        }
        bool reverseDir = other.ValidDirectionGoingTo(this);
        if (reverseDir)
        {
            other._outgoingNodes.Add(this);
            _incomingNodes.Add(other);

            other.UpdateDFS();
        }

        return forwardDir || reverseDir;
    }

    private void RemoveAllConnections()
    {
        foreach (ElectricalNode node in _outgoingNodes)
        {
            if (node != null)   //This shouldn't happen?
            {
                node._incomingNodes.Remove(this);
                node.UpdateDFS();
            }
        }
        foreach (ElectricalNode node in _incomingNodes)
        {
            if (node != null)
            {
                node._outgoingNodes.Remove(this);
                node.UpdateDFS();
            }
        }
    }

    protected bool RemoveConnection(ElectricalNode other)
    {
        if(!_outgoingNodes.Contains(other) && !other._outgoingNodes.Contains(this))
        {
            return false;
        } 

        if (_outgoingNodes.Remove(other))
        {
            other._incomingNodes.Remove(this);
        }
        if (other._outgoingNodes.Remove(this))
        {
            _incomingNodes.Remove(other);
        }

        UpdateDFS();
        other.UpdateDFS();

        return true;
    }

    public bool ValidDirectionGoingTo(ElectricalNode other)
    {
        if (other == null) {
            return false;
        }

        if (nodeType == other.nodeType)
        {
            if (nodeType == NodeType.IO)
            {
                return true;
            } else
            {
                return false;
            }
        }

        bool wrongDirection = nodeType == NodeType.OUTPUT || other.nodeType == NodeType.INPUT;
        return !wrongDirection;
    }
}