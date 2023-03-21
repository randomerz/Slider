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

    //Efficiency: (2*ai.maxDistVision-1)^2
    public override NodeState Evaluate()
    {
        int minCost = int.MaxValue;
        Vector2Int minCostPt = Vector2Int.zero;
        Vector2Int posAsInt = TileUtil.WorldToTileCoords(ai.transform.position);

        //Find the position within maxDistVision with the lowest cost on the cost map
        float dist = 0f;
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        queue.Enqueue(posAsInt);
        while (queue.Count > 0 && dist < ai.maxDistVision)
        {
            Vector2Int currPos = queue.Dequeue();
            visited.Add(currPos);
            int cost = GetTileCost(currPos);
            if (cost < int.MaxValue)   //Tile is traversable
            {
                //Since we only enqueue tiles that are traversable, there will always be a path from the start to the minCost tile that we choose
                //meaning that we no longer have to call AStar on every neighbor!
                dist = Mathf.Max(dist, Vector2Int.Distance(posAsInt, currPos));

                if (cost < minCost)
                {
                    minCost = cost;
                    minCostPt = currPos;
                }

                List<Vector2Int> neighbors = ai.nav.GetMooreNeighbors(currPos);
                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
                //Debug.Log($"Cost To: {neighbor} is {cost}");
            }
        }
        //Debug.Log($"Min Cost Neighbor: {minCostNeighbor} with cost {minCost}");
        
        //Set destination to least cost pos, WorldNavigation takes care of the rest.
        if (minCost < int.MaxValue && !minCostPt.Equals(posAsInt))
        {
            RatBlackboard.Instance.destination = minCostPt;
            RatBlackboard.Instance.costFunc = TotalCostAStar;
            return NodeState.SUCCESS;
        } else
        {
            return NodeState.FAILURE;
        }
    }

    //Total_Cost = Cost_Map_Cost + Player_Cost
    private int GetTileCost(Vector2Int pt)
    {
        Vector2Int playerPosAsInt = TileUtil.WorldToTileCoords(ai.player.position);
        //Either the Rat can't get there, or it's the player (which the Rat should avoid at all costs!)
        //bool posIllegal = !ai.CostMap.ContainsKey(pt)|| pt.Equals(playerPosAsInt);
        if (pt.Equals(playerPosAsInt))
        {
            return int.MaxValue;
        } else
        {
            float distToPlayer = Vector2Int.Distance(playerPosAsInt, pt);

            //Total cost is the cost of the tile itself (based on how close it is to a wall) 
            float normCost = ai.paintedCostMap.GetNormalizedCostAt(pt);
            if (normCost > 1.5f)
            {
                return int.MaxValue;
            }

            int tileCost = (int) (ai.paintedCostMap.GetNormalizedCostAt(pt) * ai.tileMaxPenalty);
            int playerCost = ai.CostToThreat(distToPlayer, true);
            return tileCost + playerCost;
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