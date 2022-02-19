using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayInPlaceNode : Node
{
    RatAI ai;

    public StayInPlaceNode(RatAI ai)
    {
        this.ai = ai;
    }
    public override NodeState Evaluate()
    {
        ai.SetRunning(false);
        return NodeState.RUNNING;
    }
}
