using System.Collections;
using UnityEngine;

public class MoveTowardsSetPosNode : BehaviourTreeNode
{
    private RatAI ai;

    private float updateTimer; //Controls how often the AI updates its path.

    private bool readyForUpdate;

    public MoveTowardsSetPosNode(RatAI ai, float updateTimer)
    {
        this.ai = ai;
        this.updateTimer = updateTimer;
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