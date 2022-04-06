using System.Collections.Generic;
using UnityEngine;

public class SetDestToAvoidPlayerNode : BehaviourTreeNode
{
    //The minimum range the rat can be from an obstacle before it no longer runs that way
    private RatAI ai;

    public SetDestToAvoidPlayerNode(RatAI ai)
    {
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        Vector2Int minCostNeighbor = Vector2Int.zero;
        int minCost = int.MaxValue;

        WorldNavigation nav = ai.GetComponentInParent<WorldNavigation>();
        foreach (Vector2Int neighbor in nav.GetMooreNeighbors(TileUtil.WorldToTileCoords(ai.transform.position)))
        {
            List<Vector2Int> path = new List<Vector2Int>();
            int cost = GetTileCost(neighbor);
            Debug.Log($"Cost To: {neighbor} is {cost}");
            if (cost < minCost)
            {
                minCost = cost;
                minCostNeighbor = neighbor;
            }
        }

        Debug.Log($"Min Cost Neighbor: {minCostNeighbor} with cost {minCost}");
        
        if (minCost < int.MaxValue)
        {
            RatBlackboard.Instance.destination = minCostNeighbor;
            RatBlackboard.Instance.costFunc = CostAStar;
            return NodeState.SUCCESS;
        } else
        {
            return NodeState.FAILURE;
        }
    }

    //Use this to get the cost to a point. (DON'T USE THIS TO GENERATE THE COST MAP)
    private int GetTileCost(Vector2Int pt)
    {
        Vector2Int playerPosAsInt = TileUtil.WorldToTileCoords(ai.player.position);
        if (!ai.CostMap.ContainsKey(pt) || pt.Equals(playerPosAsInt))
        {
            return int.MaxValue;
        } else
        {
            float distToPlayer = Vector2Int.Distance(playerPosAsInt, pt);

            //This can be a variety of functions of player/tile cost (max doesn't work due to corridor edge case)
            return ai.CostMap[pt] + RatAI.CostToThreat(distToPlayer);
        }
    }

    private int CostAStar(Vector2Int curr, Vector2Int neighbor, Vector2Int end)
    {

        int addCost = GetTileCost(neighbor);
        if (addCost == int.MaxValue)
        {
            return int.MaxValue;
        } else
        {
            return WorldNavigation.GetAStarCost(curr, neighbor, end) + addCost;
        }
    }
}