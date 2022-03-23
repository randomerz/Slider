using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowardsObjectNode : BehaviourTreeNode
{
    //The minimum range the rat can be from an obstacle before it no longer runs that way
    RatAI ai;

    public MoveTowardsObjectNode(RatAI ai)
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
            if (Vector2.Distance(ai.transform.position, ai.objectToSteal.transform.position) < ai.navAgent.tolerance)
            {
                return NodeState.SUCCESS;
            }

            ai.navAgent.SetDestination(ai.objectToSteal.transform.position);
            ai.navAgent.FollowPath();
            return NodeState.RUNNING;
        }


    }
}

