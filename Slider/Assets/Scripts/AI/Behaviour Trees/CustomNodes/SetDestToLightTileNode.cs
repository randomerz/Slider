using System.Collections.Generic;
using UnityEngine;

public class SetDestToLightTileNode : BehaviourTreeNode
{
    //The minimum range the rat can be from an obstacle before it no longer runs that way
    private RatAI ai;

    public SetDestToLightTileNode(RatAI ai)
    {
        this.ai = ai;
    }

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
    private List<Vector2Int> FindNearestValidLightTile(Vector2Int posAsInt)
    {
        var result = new List<Vector2Int>();    //This is kind of a workaround since Vector2Int can't be null.
        WorldNavigation nav = ai.GetComponentInParent<WorldNavigation>();

        int dist = 1;
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        visited.Add(posAsInt);
        queue.Enqueue(posAsInt);
        while (queue.Count > 0 && dist < ai.GetComponentInParent<STile>().STILE_WIDTH * 1.41f)   //worst case scenario it's in the corner and has to check up to the opposite corner
        {
            Vector2Int currPos = queue.Dequeue();

            Vector2Int[] neighborDirs = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right,
                                      new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

            foreach (var dir in neighborDirs)
            {
                Vector2Int posToCheck = currPos + dir;
                if (!visited.Contains(posToCheck))
                {
                    visited.Add(posToCheck);
                    queue.Enqueue(posToCheck);
                    if (nav.ValidPts.Contains(posToCheck) && LightManager.instance.GetLightMaskAt(posToCheck.x, posToCheck.y))
                    {
                        List<Vector2Int> path = new List<Vector2Int>();
                        if (nav.GetPathFromToAStar(posAsInt, posToCheck, out path))
                        {
                            //It has to be possible to actually get there in order for this to be valid.
                            result.Add(posToCheck);
                            return result;
                        }
                    }
                }

                dist = (int)Vector2Int.Distance(posAsInt, posToCheck);
            }
        }

        ai.visited = visited;   //To draw gizmos for debugging purposes
        return null;
    }
}

