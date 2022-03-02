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
            if (Vector2.Distance(ai.transform.position, ai.objectToSteal.transform.position) < 0.1f)
            {
                return NodeState.SUCCESS;
            }

            Vector2 dirTowardsObject = (ai.objectToSteal.transform.position - ai.transform.position).normalized;
            ai.SetDirection(dirTowardsObject);
            ai.Move();
            return NodeState.RUNNING;
        }


    }
}

