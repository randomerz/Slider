using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MoveTowardsObjectNode : BehaviourTreeNode
{
    //The minimum range the rat can be from an obstacle before it no longer runs that way
    private RatAI ai;

    private const float updateTimer = 1.0f;

    private bool readyForUpdate;

    public MoveTowardsObjectNode(RatAI ai)
    {
        this.ai = ai;
        readyForUpdate = true;
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

            if (readyForUpdate)
            {
                ai.StartCoroutine(UpdateAsync());
            }

            return ai.navAgent.IsRunning ? NodeState.RUNNING : NodeState.FAILURE;
        }
    }

    private IEnumerator UpdateAsync()
    {
        ai.navAgent.SetDestination(ai.objectToSteal.transform.position);
        readyForUpdate = false;
        yield return new WaitForSeconds(updateTimer);
        readyForUpdate = true;
    }
}

