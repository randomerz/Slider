using System.Collections;
using UnityEngine;

public class SetDestToObjectNode : BehaviourTreeNode
{
    //The minimum range the rat can be from an obstacle before it no longer runs that way
    private RatAI ai;

    public SetDestToObjectNode(RatAI ai)
    {
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        if (ai.holdingObject)
        {
            return NodeState.FAILURE;
        } else
        {
            RatBlackboard.Instance.destination = ai.collectibleToSteal.transform.position;
            return NodeState.SUCCESS;
        }
    }
}

