using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorNode : BehaviourTreeNode
{
    protected List<BehaviourTreeNode> childNodes;

    public SelectorNode(List<BehaviourTreeNode> nodes)
    {
        this.childNodes = nodes;
    }

    public SelectorNode() : this(new List<BehaviourTreeNode>())
    {
        
    }

    public override NodeState Evaluate()
    {
        //L: Effectively Acts as an "OR Gate" between child nodes

        _state = NodeState.RUNNING;
        foreach (var node in childNodes)
        {
            //L: Evaluate all child nodes
            switch (node.Evaluate())
            {
                case NodeState.RUNNING:
                    _state = NodeState.RUNNING;
                    return _state;
                case NodeState.SUCCESS:
                    _state = NodeState.SUCCESS;
                    return _state;
                case NodeState.FAILURE:
                    break;
                default:
                    break;

            }
        }

        //L: No successes after loop bc otherwise the loop would have terminated
        _state = NodeState.FAILURE;
        return _state;
    }
}
