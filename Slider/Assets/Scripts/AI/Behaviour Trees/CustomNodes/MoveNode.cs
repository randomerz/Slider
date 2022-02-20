using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveNode : BehaviourTreeNode
{
    private RatAI ai;

    public MoveNode(RatAI ai)
    {
        this.ai = ai;
    }
    public override NodeState Evaluate()
    {
        ai.Move();
        return NodeState.RUNNING;
    }
}