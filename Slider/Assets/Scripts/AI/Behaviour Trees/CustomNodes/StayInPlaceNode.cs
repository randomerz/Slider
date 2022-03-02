using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayInPlaceNode : BehaviourTreeNode
{
    RatAI ai;

    public StayInPlaceNode(RatAI ai)
    {
        this.ai = ai;
    }
    public override NodeState Evaluate()
    {
        ai.Stay();
        return NodeState.RUNNING;
    }
}
