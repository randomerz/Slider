using System.Collections;
using UnityEngine;

public class MoveTowardsSetPosNode : BehaviourTreeNode
{
    //The minimum range the rat can be from an obstacle before it no longer runs that way
    private RatAI ai;

    private const float updateTimer = 0.1f;

    private bool readyForUpdate;

    public MoveTowardsSetPosNode(RatAI ai)
    {
        this.ai = ai;
        readyForUpdate = true;
    }

    public override NodeState Evaluate()
    {
        if (Vector2.Distance(ai.transform.position, RatBlackboard.Instance.destination) < ai.navAgent.tolerance)
        {
            ai.navAgent.StopPath();
            return NodeState.SUCCESS;
        }

        if (readyForUpdate)
        {
            ai.navAgent.SetDestination(TileUtil.WorldToTileCoords(RatBlackboard.Instance.destination), RatBlackboard.Instance.costFunc);
            ai.StartCoroutine(WaitAsync());
        }

        return ai.navAgent.IsRunning ? NodeState.RUNNING : NodeState.FAILURE;
    }

    private IEnumerator WaitAsync()
    {
        readyForUpdate = false;
        yield return new WaitForSeconds(updateTimer);
        readyForUpdate = true;
    }
}