using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceNode : Node
{
    protected List<Node> childNodes;

    public SequenceNode(List<Node> nodes)
    {
        this.childNodes = nodes;
    }

    public SequenceNode() : this(new List<Node>())
    {
        
    }

    public override NodeState Evaluate()
    {
        //L: Effectively Acts as an "AND Gate" between child nodes

        _state = NodeState.RUNNING;
        bool childNodeStillRunning = false;
        foreach (var node in childNodes)
        {
            //L: Evaluate all child nodes
            switch (node.Evaluate())
            {
                case NodeState.RUNNING:
                    childNodeStillRunning = true;
                    break;
                case NodeState.SUCCESS:
                    break;
                case NodeState.FAILURE:
                    _state = NodeState.FAILURE;
                    return _state;
                default:
                    break;

            }
        }

        //L: No failures after loop bc otherwise the loop would have terminated
        _state = childNodeStillRunning ? NodeState.RUNNING : NodeState.SUCCESS;
        return _state;
    }
}
