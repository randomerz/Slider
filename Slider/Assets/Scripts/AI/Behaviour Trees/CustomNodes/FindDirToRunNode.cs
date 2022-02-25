using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindDirToRunNode : BehaviourTreeNode
{
    //The minimum range the rat can be from an obstacle before it no longer runs that way
    RatAI ai;
    Transform player;

    private float minDistToWall;

    private Dictionary<Vector2, float> distanceCache;   //Cache to avoid raycasting the same direction multiple times

    public FindDirToRunNode(RatAI ai, Transform player)
    {
        this.ai = ai;
        this.player = player;
        minDistToWall = ai.moveSpeed * ai.minDistToWallFactor;
    }

    public override NodeState Evaluate()
    {
        //Renew the distance cache on each evaluate (might not need to do it every time)
        distanceCache = GetDistancesInAllDirections(PlayerToRatNorm()); 

        Vector2 avoidPlayerDir = GetBestDirAwayFromPoint(player.transform.position);
        Vector2 avoidWallsDir = GetBestDirAwayFromWalls();

        if (avoidPlayerDir == Vector2.zero && avoidPlayerDir == Vector2.zero)
        {
            //Rat was cornered completely.
            return NodeState.FAILURE;
        } else
        {
            Debug.Log("Avoid Player: " + avoidPlayerDir);
            Debug.Log("Avoid Walls: " + avoidWallsDir);
            Vector2 idealDir = Vector2.Lerp(avoidPlayerDir, avoidWallsDir, ai.avoidWallsWeight);
            ai.SetDirection(CheckValidDir(idealDir) ? idealDir : avoidPlayerDir);   //Avoid the player, regardless of weight
            return NodeState.SUCCESS;
        }
        
    }

    private Vector2 GetBestDirAwayFromPoint(Vector2 point, int numDirections = 16)
    {
        Vector2 bestDir = ((Vector2) ai.transform.position - point).normalized;
        Vector2 currDir1 = Vector2.zero;
        Vector2 currDir2 = Vector2.zero;

        for (float theta = 0; theta < Mathf.PI - 0.0001f; theta += 2 * Mathf.PI / numDirections)
        {
            //L: Check on both sides of playerToRat for openings.
            currDir1 = Vector2Rotate.Rotate(bestDir, theta);
            if (CheckValidDir(currDir1))
            {
                return currDir1.normalized;
            }
            currDir2 = Vector2Rotate.Rotate(bestDir, -theta);
            if (CheckValidDir(currDir2))
            {
                return currDir2.normalized;
            }
        }

        return Vector2.zero;
    }

    private Vector2 GetBestDirAwayFromWalls(int numDirections = 16)
    {
        //"Best" is direction furthest away from walls or direction that avoids the closest wall.
        Vector2 bestDir = Vector2.zero;
        float maxDist = 0;

        Vector2 closestWallDir = Vector2.zero;
        float minDist = Mathf.Infinity;

        //First Pass: Largest distance from wall to rat within the ideal distance (distances greater than ideal are tied).
        HashSet<Vector2> secondPassDirs = new HashSet<Vector2>();
        foreach (Vector2 dir in distanceCache.Keys)
        {
            float dist = distanceCache[dir];
            Debug.Log("Distance: " + dist);
            if (dist < minDist)
            {
                minDist = dist;
                closestWallDir = dir;
            }

            if (dist > minDistToWall && dist >= maxDist)
            {
                if (dist > maxDist)
                {
                    maxDist = dist;
                    secondPassDirs.Clear();
                }

                secondPassDirs.Add(dir);
            }
        }

        //Second Pass: Most negative dot product from the closest wall 
        float minDot = Mathf.Infinity;
        foreach (Vector2 dir in secondPassDirs)
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
        return GetDistToWallOrDark(dir) > minDistToWall;
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
            RaycastHit2D[] hits = new RaycastHit2D[1];  //We only care about the first hit.
            int numResults = Physics2D.Raycast(ai.transform.position, dir, GetRaycastFilter(), hits, ai.idealDistFromWall);
            bool lit = true;
            if (LightManager.instance != null)
            {
                Vector2 pos = (Vector2)ai.transform.position + dir;
                lit = LightManager.instance.GetLightMaskAt((int)pos.x, (int)pos.y);
            }

            float dist = numResults == 0 ? Mathf.Infinity : Vector2.Distance(hits[0].point, ai.transform.position);
            if (cache)
            {
                distanceCache[dir] = dist;
            }

            return dist;
        }

        return distanceCache[dir];
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