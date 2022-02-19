using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsTargetCloseNode : Node
{
    private float range;
    private Transform source;
    private Transform target;

    public IsTargetCloseNode(Transform source, Transform target, float range)
    {
        this.source = source;
        this.target = target;
        this.range = range;
    }
    public override NodeState Evaluate()
    {
            return Vector3.Distance(target.position, source.position) < range ?
                NodeState.SUCCESS : NodeState.FAILURE;
    }
}
