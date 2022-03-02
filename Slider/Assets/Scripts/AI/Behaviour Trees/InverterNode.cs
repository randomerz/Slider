using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverterNode : BehaviourTreeNode
{
    protected BehaviourTreeNode child;

    public InverterNode(BehaviourTreeNode child)
    {
        this.child = child;
    }

    public override NodeState Evaluate()
    {
        //L: Acts as an "NOT Gate" for the input
        switch (child.Evaluate())
        {
            case NodeState.RUNNING:
                _state = NodeState.RUNNING;
                break;
            case NodeState.SUCCESS:
                _state = NodeState.FAILURE;
                break;
            case NodeState.FAILURE:
                _state = NodeState.SUCCESS;
                break;
            default:
                break;
        }
        return _state;
    }
}
