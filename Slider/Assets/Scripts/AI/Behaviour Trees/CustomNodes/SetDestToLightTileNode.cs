﻿using System.Collections.Generic;
using UnityEngine;

public class SetDestToLightTileNode : BehaviourTreeNode
{
    //The minimum range the rat can be from an obstacle before it no longer runs that way
    private RatAI ai;

    public SetDestToLightTileNode(RatAI ai)
    {
        this.ai = ai;
    }

    //Efficiency: (2 * ai.maxDistVision + 1)^2
    public override NodeState Evaluate()
    {
        Vector2Int posAsInt = TileUtil.WorldToTileCoords(ai.transform.position);
        if (LightManager.instance.GetLightMaskAt(posAsInt.x, posAsInt.y))   //Rat is already standing in light
        {
            return NodeState.FAILURE;
        }
        else
        {
            List<Vector2Int> nearestLightTile = FindNearestValidLightTile(posAsInt);
            if (nearestLightTile == null)
            {
                return NodeState.FAILURE;
            }

            RatBlackboard.Instance.destination = nearestLightTile[0];
            RatBlackboard.Instance.costFunc = null;
            return NodeState.SUCCESS;
        }
    }

    //This algorithm essentially checks the given pos, it's neighbors, the neighbors' neighbors, and so on moving outwards from the original pos.
    //512 calculations
    private List<Vector2Int> FindNearestValidLightTile(Vector2Int posAsInt)
    {
        var result = new List<Vector2Int>();    //This is kind of a workaround since Vector2Int can't be null.

        float dist = 0f;
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        visited.Add(posAsInt);
        queue.Enqueue(posAsInt);
        while (queue.Count > 0 && dist < ai.maxDistVision)   //Checks up to half a tile width.
        {
            Vector2Int currPos = queue.Dequeue();
            List<Vector2Int> neighbors = ai.nav.GetMooreNeighbors(currPos);

            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    dist = Mathf.Max(dist, Vector2Int.Distance(posAsInt, neighbor));
                    queue.Enqueue(neighbor);
                    if (LightManager.instance.GetLightMaskAt(neighbor.x, neighbor.y))
                    {
                        result.Add(neighbor);
                        return result;
                    }
                }
            }
        }

        ai.visited = visited;   //To draw gizmos for debugging purposes
        return null;
    }
}

