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
            int radius = 1;
            var queue = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();

            visited.Add(posAsInt);
            queue.Enqueue(posAsInt);
            while (queue.Count > 0 && radius < ai.GetComponentInParent<STile>().STILE_WIDTH * 1.41f)   //worst case scenario it's in the corner and has to check up to the opposite corner
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
                        if (LightManager.instance.GetLightMaskAt(posToCheck.x, posToCheck.y))
                        {
                            //Debug.Log("Found Lit Tile: " + posToCheck);
                            STileNavigation nav = ai.GetComponentInParent<STileNavigation>();
                            List<Vector2Int> path = nav.GetPathFromToHard(posAsInt, posToCheck);

                            if (path != null)
                            {
                                //It has to be possible to actually get there in order for this to be valid.
                                //Debug.Log("Found Path");
                                RatBlackboard.Instance.destination = posToCheck;
                                return NodeState.SUCCESS;
                            }
                        }
                    }

                }
                radius++;
            }

            return NodeState.FAILURE;
        }
    }
}

