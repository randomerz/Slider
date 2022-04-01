using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroAtProximityNode : BehaviourTreeNode
{
    private float aggroRange;
    private float deaggroRange;
    private Transform source;
    private Transform target;

    private bool aggro;

    public AggroAtProximityNode(Transform source, Transform target, float aggroRange, float deaggroRange)
    {
        this.source = source;
        this.target = target;
        this.aggroRange = aggroRange;
        this.deaggroRange = deaggroRange;
        aggro = false;
    }
    public override NodeState Evaluate()
    {
        float dist = Vector3.Distance(target.position, source.position);
        if (aggro && dist < deaggroRange)
        {
            return NodeState.SUCCESS;
        }

        if (!aggro && dist < aggroRange)
        {
            aggro = true;
            return NodeState.SUCCESS;
        }

        aggro = false;
        return NodeState.FAILURE;
    }
}
