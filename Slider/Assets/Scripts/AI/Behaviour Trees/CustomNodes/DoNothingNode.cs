using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNothingNode : BehaviourTreeNode
{
    RatAI ai;

    public DoNothingNode(RatAI ai)
    {
        this.ai = ai;
    }
    public override NodeState Evaluate()
    {
        ai.navAgent.StopPath();
        //ai.transform.up = (ai.player.transform.position - ai.transform.position).normalized;
        return NodeState.RUNNING;
    }
}
