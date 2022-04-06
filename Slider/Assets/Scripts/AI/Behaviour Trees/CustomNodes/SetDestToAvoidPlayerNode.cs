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
        Vector2Int posAsInt = TileUtil.WorldToTileCoords(ai.transform.position);

        float dist = 0f;
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        visited.Add(posAsInt);
        queue.Enqueue(posAsInt);
        while (queue.Count > 0 && dist < RatAI.maxDistVision)   //worst case scenario it's in the corner and has to check up to the opposite corner
        {
            Vector2Int currPos = queue.Dequeue();

            List<Vector2Int> neighbors = nav.GetMooreNeighbors(currPos);

            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    dist = Vector2Int.Distance(posAsInt, neighbor);
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);

                    int cost = GetTileCost(neighbor);
                    //Debug.Log($"Cost To: {neighbor} is {cost}");
                    List<Vector2Int> path;  //Don't actually care about this
                    if (cost < minCost)
                    {
                        if (dist < 1.5f || nav.GetPathFromToAStar(posAsInt, neighbor, out path, false, CostAStar))
                        {
                            minCost = cost;
                            minCostNeighbor = neighbor;
                        }
                    }
                }
            }
        }

        //Debug.Log($"Min Cost Neighbor: {minCostNeighbor} with cost {minCost}");
        
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