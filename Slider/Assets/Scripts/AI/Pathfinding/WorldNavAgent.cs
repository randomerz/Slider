using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

//Attach this to the agent you want to use STileNavigation (or WorldNavigation)
public class WorldNavAgent : MonoBehaviour
{
    public float speed;
    public float tolerance;

    private List<Vector2Int> path;

    private Vector2Int currDest;

    private Coroutine followRoutine;

    private Rigidbody2D rb;

    private WorldNavigation nav;

    [Header("DEBUG RAT")]
    public RatAI ai; // debug

    public bool IsRunning
    {
        get;
        private set;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("WorldNavAgent requires Rigidbody2D");
        }

        nav = GetComponentInParent<WorldNavigation>();
        if (nav == null)
        {
            // DC: silence, warning
            //Debug.LogWarning("WorldNavAgent detected in scene without a WorldNavigation component. This entity will not use navigation.");
        }
    }

    public bool SetDestination(Vector2Int dest, Func<Vector2Int, Vector2Int, Vector2Int, int> costFunc = null, Action<Vector2Int> callback = null)
    {
        //Get the closest point to this transform
        Vector2Int posAsInt = TileUtil.WorldToTileCoords(transform.position);
        nav.GetPathFromToAStar(posAsInt, dest, out path, false, costFunc);
        //Debug.Log($"Path To {dest}:");
        //foreach (Vector2Int pos in path)
        //{
        //    Debug.Log(pos);
        //}

        if (IsRunning)
        {
            // Debug.Log("called stop path bc new destination");
            StopPath();
        }

        if (path == null || path.Count == 0)
        {
            return false;
        }
        currDest = dest;
        followRoutine = StartCoroutine(FollowCoroutine(callback));
        return true;
    }

    private IEnumerator FollowCoroutine(Action<Vector2Int> callback = null)
    {
        IsRunning = true;
        while(path.Count > 0)
        {
            while (Vector2.Distance(path[0], transform.position) > tolerance)
            {
                rb.velocity = (path[0] - (Vector2)transform.position).normalized * speed;
                yield return null;
            }

            path.RemoveAt(0);
        }

        while (Vector2.Distance(transform.position, currDest) > tolerance)
        {
            rb.velocity = (currDest - (Vector2)transform.position).normalized * speed;
            yield return null;
        }

        IsRunning = false;
        rb.velocity = Vector2.zero;

        if (callback != null)
        {
            callback(currDest);
        }
    }

    public void StopPath()
    {
        if (followRoutine != null)
        {
            StopCoroutine(followRoutine);
        }

        IsRunning = false;
        rb.velocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        //Draws out the path
        if (path != null)
        {
            for (int i=0; i<path.Count; i++)
            {
                Gizmos.color = Color.green;
                //Gizmos.DrawSphere(new Vector3(path[i].x, path[i].y, 0), 0.2f);

                if (i != path.Count - 1)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(new Vector3(path[i].x, path[i].y, 0), new Vector3(path[i+1].x, path[i+1].y, 0));
                }
            }
        }

        // // super laggy debug mode B)

        // int minCost = int.MaxValue;
        // Vector2Int minCostPt = Vector2Int.zero;
        // Vector2Int posAsInt = TileUtil.WorldToTileCoords(transform.position);

        // //Find the position within maxDistVision with the lowest cost on the cost map
        // float dist = 0f;
        // var queue = new Queue<Vector2Int>();
        // var visited = new HashSet<Vector2Int>();
        // queue.Enqueue(posAsInt);
        // while (queue.Count > 0 && dist < ai.maxDistVision + 2)
        // {
        //     Vector2Int currPos = queue.Dequeue();
        //     visited.Add(currPos);
        //     int cost = GetTileCost(currPos);
        //     if (cost < int.MaxValue)   //Tile is traversable
        //     {
        //         Gizmos.color = Color.Lerp(Color.green, Color.red, cost / 300f);
        //         // if (cost != 0) Debug.Log(cost);
        //         Gizmos.DrawSphere(new Vector3(currPos.x, currPos.y), 0.125f + cost / 1500f);

        //         //Since we only enqueue tiles that are traversable, there will always be a path from the start to the minCost tile that we choose
        //         //meaning that we no longer have to call AStar on every neighbor!
        //         dist = Mathf.Max(dist, Vector2Int.Distance(posAsInt, currPos));

        //         if (cost < minCost)
        //         {
        //             minCost = cost;
        //             minCostPt = currPos;
        //         }

        //         List<Vector2Int> neighbors = ai.nav.GetMooreNeighbors(currPos);
        //         foreach (var neighbor in neighbors)
        //         {
        //             if (!visited.Contains(neighbor))
        //             {
        //                 queue.Enqueue(neighbor);
        //             }
        //         }
        //         //Debug.Log($"Cost To: {neighbor} is {cost}");
        //     }
        // }
    }
    
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
}