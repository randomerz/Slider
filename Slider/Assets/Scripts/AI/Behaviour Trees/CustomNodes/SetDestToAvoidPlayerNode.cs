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
        while (queue.Count > 0 && dist < ai.maxDistVision)
        {
            Vector2Int currPos = queue.Dequeue();

            List<Vector2Int> neighbors = nav.GetMooreNeighbors(currPos);
            dist = Vector2Int.Distance(posAsInt, neighbors[0]);

            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);

                    int cost = GetTileCost(neighbor);
                    //Debug.Log($"Cost To: {neighbor} is {cost}");
                    List<Vector2Int> path;  //Don't actually care about this
                    if (cost < minCost)
                    {
                        if (dist < 1.5f || nav.GetPathFromToAStar(posAsInt, neighbor, out path, false, TotalCostAStar))
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
            RatBlackboard.Instance.costFunc = TotalCostAStar;
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
        if (!ai.CostMap.ContainsKey(pt) || pt.Equals(playerPosAsInt))   //Either the Rat can't get there, or it's the player (which the Rat should avoid at all costs!)
        {
            return int.MaxValue;
        } else
        {
            float distToPlayer = Vector2Int.Distance(playerPosAsInt, pt);

            //Total cost is the cost of the tile itself (based on how close it is to a wall) 
            return ai.CostMap[pt] + ai.CostToThreat(distToPlayer);
        }
    }

    private int TotalCostAStar(Vector2Int curr, Vector2Int neighbor, Vector2Int end)
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