using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindDirToRunNode : BehaviourTreeNode
{
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
        //Check if the rat is on a collision course to the player

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
            Debug.Log("Avoid Player: " + avoidPlayerDir);
            Debug.Log("Avoid Walls: " + avoidWallsDir);

            if (avoidWallsDir.magnitude < 0.1f)
            {
                ai.SetDirection(avoidPlayerDir);
            } else
            {
                //Vector2 idealDir = Vector2.Lerp(avoidPlayerDir, avoidWallsDir, ai.avoidWallsWeight);
                Vector2 idealDir = avoidPlayerDir;
                ai.SetDirection(idealDir);   //Avoid the player, regardless of weight
            }

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
            //Raycast to walls
            RaycastHit2D[] hits = new RaycastHit2D[1];  //We only care about the first hit.
            int numResults = Physics2D.Raycast(ai.transform.position, dir, GetRaycastFilter(), hits, ai.idealDistFromWall);
            float distToWall = numResults == 0 ? Mathf.Infinity : Vector2.Distance(hits[0].point, ai.transform.position);

            //Raycast on lightmap
            float distToShadow = Mathf.Infinity;
            if (LightManager.instance != null)
            {
                distToShadow = LightRaycast(dir);
                Debug.Log(distToShadow);
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
        Vector2Int pos = TileUtil.WorldToTileCoords(ai.transform.position);
        Vector2Int prevPos;
        float dist = 0;
        do
        {
            prevPos = pos;
            pos = TileUtil.WorldToTileCoords(pos + dir * 1.5f);
            dist += (pos - prevPos).magnitude;  //This should either be 1 or sqrt(2)
        } while (dist < ai.idealDistFromWall && LightManager.instance.GetLightMaskAt(pos.x, pos.y));

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
        filter.layerMask = ~LayerMask.GetMask("Ignore Raycast", "SlideableArea", "Player", "Rat");

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