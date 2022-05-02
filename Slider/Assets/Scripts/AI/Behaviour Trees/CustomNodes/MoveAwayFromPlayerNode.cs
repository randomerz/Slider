using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* No longer used in favor of CostMap method.
public class MoveAwayFromPlayerNode : BehaviourTreeNode
{
    RatAI ai;
    Transform player;

    private Dictionary<Vector2, float> distanceCache;   //Cache to avoid raycasting the same direction multiple times

    public MoveAwayFromPlayerNode(RatAI ai, Transform player)
    {
        this.ai = ai;
        this.player = player;
    }

    public override NodeState Evaluate()
    {
        ai.navAgent.StopPath(); //ai needs to run away, so pathing is no longer relevant.

        //Renew the distance cache on each evaluate (might not need to do it every time)
        distanceCache = GetDistancesInAllDirections(PlayerToRatNorm()); 

        Vector2 avoidPlayerDir = GetBestDirAwayFromPoint(player.transform.position);

        if (avoidPlayerDir == Vector2.zero || Vector2.Dot(avoidPlayerDir, PlayerToRatNorm()) < -0.9f)
        {
            //Rat was cornered completely.
            ai.SetDirection(-PlayerToRatNorm());
            return NodeState.FAILURE;
        } else
        {

            Vector2 avoidWallsDir = GetBestDirAwayFromWalls();
            //Debug.Log("Avoid Player: " + avoidPlayerDir);
            //Debug.Log("Avoid Walls: " + avoidWallsDir);

            if (avoidWallsDir.magnitude < 0.1f)
            {
                ai.SetDirection(avoidPlayerDir);
            } else
            {
                Vector2 idealDir = Vector2.Lerp(avoidPlayerDir, avoidWallsDir, ai.avoidWallsWeight);
                ai.SetDirection(idealDir);
            }

            ai.Move();
            return NodeState.SUCCESS;
        }
        
    }

    private Vector2 GetBestDirAwayFromPoint(Vector2 point, int numDirections = 16)
    {
        Vector2 bestDir = ((Vector2) ai.transform.position - point).normalized;
        Vector2 currDir1 = Vector2.zero;
        Vector2 currDir2 = Vector2.zero;

        List<Vector2> candidateDirs = new List<Vector2>(2 * numDirections);
        for (float theta = 0; theta < Mathf.PI - 0.0001f; theta += 2 * Mathf.PI / numDirections)
        {
            //L: Check on both sides of playerToRat for openings.
            currDir1 = Vector2Rotate.Rotate(bestDir, theta).normalized;
            if (CheckValidDir(currDir1))
            {
                candidateDirs.Add(currDir1);
            }
            currDir2 = Vector2Rotate.Rotate(bestDir, -theta).normalized;
            if (CheckValidDir(currDir2))
            {
                candidateDirs.Add(currDir2);
            }
        }

        //Debug.Log(candidateDirs.Count);

        if (candidateDirs.Count == 0)
        {
            return Vector2.zero;
        }

        Vector2 closestToFacingDir = Vector2.zero;
        float closestToFacingDot = Mathf.NegativeInfinity;
        foreach (Vector2 dir in candidateDirs)
        {
            float dot = Vector2.Dot(dir, ai.DirectionFacing);
            if (dot > closestToFacingDot)
            {
                closestToFacingDir = dir;
                closestToFacingDot = dot;
            }
        }

        return closestToFacingDot < ai.decisiveness ? closestToFacingDir : candidateDirs[0];
    }

    private Vector2 GetBestDirAwayFromWalls(int numDirections = 16)
    {

        Vector2 closestWallDir = Vector2.zero;
        float minDist = Mathf.Infinity;

        //First Pass: Largest distance from wall to rat within the ideal distance (distances greater than ideal are tied).
        
        foreach (Vector2 dir in distanceCache.Keys)
        {
            float dist = distanceCache[dir];
            if (dist < minDist)
            {
                minDist = dist;
                closestWallDir = dir;
            }
           // Debug.Log("Direction: " + dir + " with distance " + distanceCache[dir]);
        }

        if (closestWallDir == Vector2.zero)
        {
            return Vector2.zero;
        }

        //Second Pass: Most negative dot product from the closest wall 
        Vector2 bestDir = Vector2.zero;
        float minDot = Mathf.Infinity;
        foreach (Vector2 dir in distanceCache.Keys)
        {
            float currDot = Vector2.Dot(dir, closestWallDir);
            if (currDot < minDot)
            {
                minDot = currDot;
                bestDir = dir;
            }
        }

        return bestDir.normalized;
    }

    //Whether a direction is valid based on if it is far enough from the wall.
    private bool CheckValidDir(Vector2 dir)
    {
        return GetDistToWallOrDark(dir) > ai.minDistToWall;
    }

    private Vector2 PlayerToRatNorm()
    {
        return (ai.transform.position - player.transform.position).normalized;
    }

    //Performs a raycast to determine the distance to a wall
    private float GetDistToWallOrDark(Vector2 dir, bool cache=true)
    {
        if (cache && distanceCache == null)
        {
            Debug.LogError("Distance cache on the Rat is null");
        }
        if (!cache || !distanceCache.ContainsKey(dir))
        {
            //Raycast to walls
            RaycastHit2D[] hits = new RaycastHit2D[1];  //We only care about the first hit.
            Collider2D collider = ai.GetComponent<BoxCollider2D>();
            int numResults = collider.Raycast(dir, GetRaycastFilter(), hits, ai.idealDistFromWall);
            float distToWall = numResults == 0 ? Mathf.Infinity : Vector2.Distance(hits[0].point, ai.transform.position);

            //Raycast on lightmap
            float distToShadow = Mathf.Infinity;
            if (LightManager.instance != null)
            {
                distToShadow = LightRaycast(dir);
                //Debug.Log("Distance from rat to shadow: " + distToShadow);
            }

            float dist = Mathf.Min(distToWall, distToShadow);
            if (cache)
            {
                distanceCache[dir] = dist;
            }

            return dist;
        }

        return distanceCache[dir];
    }

    private float LightRaycast(Vector2 dir) 
    {
        Vector2Int nextTilePos = TileUtil.WorldToTileCoords((Vector2) ai.transform.position + dir);
        Vector2Int prevPos;
        float dist = (nextTilePos - (Vector2) ai.transform.position).magnitude;
        while(dist < ai.idealDistFromWall && LightManager.instance.GetLightMaskAt(nextTilePos.x, nextTilePos.y))
        {
            prevPos = nextTilePos;
            nextTilePos = TileUtil.WorldToTileCoords(prevPos + dir);
            dist += (nextTilePos - prevPos).magnitude;
        }

        if (dist > ai.idealDistFromWall)
        {
            dist = Mathf.Infinity;
        }
        return dist;
    }

    private ContactFilter2D GetRaycastFilter()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter = filter.NoFilter();
        filter.useTriggers = false;
        filter.useLayerMask = true;
        filter.layerMask = ~LayerMask.GetMask("Ignore Raycast", "SlideableArea", "Rat");

        return filter;
    }

    private Dictionary<Vector2, float> GetDistancesInAllDirections(Vector2 start, int numDirections = 16)
    {
        var distances = new Dictionary<Vector2, float>();

        for (float theta = 0; theta < 2 * Mathf.PI - 0.0001f; theta += Mathf.PI * 2 / numDirections)
        {
            Vector2 currDir = Vector2Rotate.Rotate(start, theta);
            distances.Add(currDir, GetDistToWallOrDark(currDir, false));
        }

        return distances;
    }
}
*/