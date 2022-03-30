using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Priority_Queue;

public class WorldNavigation : MonoBehaviour
{
    [SerializeField]
    private Tilemap worldFloorTM;

    //L: Why use a graph when you can use a set of pts?
    private HashSet<Vector2Int> validPts;

    private HashSet<Vector2Int> validPtsWorld;
    private Dictionary<STile, HashSet<Vector2Int>> validPtsStiles;

    [Header("Debug")]
    [SerializeField]
    private Vector2Int debugFrom;
    [SerializeField]
    private Vector2Int debugTo;

    private List<Vector2Int> debugPath;

    private void Awake()
    {
        validPts = new HashSet<Vector2Int>();
        validPtsWorld = new HashSet<Vector2Int>();
        validPtsStiles = new Dictionary<STile, HashSet<Vector2Int>>();
        ConstructValidPts();
    }

    private void OnEnable()
    {
        SGrid.OnSTileEnabled += OnTileEnabledHandler;
        SGridAnimator.OnSTileMoveEnd += OnTileMovedHandler;
    }

    private void OnDisable()
    {
        SGrid.OnSTileEnabled -= OnTileEnabledHandler;
        SGridAnimator.OnSTileMoveEnd -= OnTileMovedHandler;
    }

    private void OnTileEnabledHandler(object sender, SGrid.OnSTileEnabledArgs e)
    {
        ConstructValidPts();
    }

    private void OnTileMovedHandler(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        ConstructValidPts();
    }

    private void ConstructValidPts()
    {
        validPts = new HashSet<Vector2Int>();
        if (validPtsWorld == null || validPtsWorld.Count == 0)
        {
            validPtsWorld = GetWorldValidPts();
        }

        validPts.UnionWith(validPtsWorld);

        STile[] stiles = GetComponentsInChildren<STile>();
        foreach (STile stile in stiles)
        {
            if (stile.isTileActive)
            {
                validPts.UnionWith(GetSTileValidPtsHard(stile));
            }
        }
    }

    private HashSet<Vector2Int> GetWorldValidPts()
    {
        var result = new HashSet<Vector2Int>();
        ContactFilter2D filter = GetFilterWithoutTriggers(~LayerMask.GetMask("Ignore Raycast", "SlideableArea", "Player", "Rat"));
        RaycastHit2D[] hits = new RaycastHit2D[1];
        foreach (Vector2Int pos in worldFloorTM.cellBounds.allPositionsWithin)
        {
            STile stile = GetComponentInChildren<STile>();
            //This assumes bottom left tile is at (0, 0)
            if (pos.x < -stile.STILE_WIDTH / 2 || pos.x > 2 * stile.STILE_WIDTH + stile.STILE_WIDTH / 2
             || pos.y < -stile.STILE_WIDTH / 2 || pos.y > 2 * stile.STILE_WIDTH + stile.STILE_WIDTH / 2)
            {
                int hit = Physics2D.CircleCast(pos, 0.5f, Vector2.up, filter, hits, 0f);
                if (hit == 0)
                {
                    result.Add(pos);
                }
            }
        }

        return result;
    }

    private HashSet<Vector2Int> GetSTileValidPtsHard(STile stile)
    {
        HashSet<Vector2Int> result = new HashSet<Vector2Int>();
        if (!validPtsStiles.ContainsKey(stile))
        {
            validPtsStiles.Add(stile, GetSTileValidPtsRelative(stile));
        }

        foreach (Vector2Int pt in validPtsStiles[stile])
        {
            result.Add(pt + new Vector2Int((int)stile.transform.position.x, (int)stile.transform.position.y));
        }

        return result;
    }

    private HashSet<Vector2Int> GetSTileValidPtsRelative(STile stile)
    {
        var result = new HashSet<Vector2Int>();

        //Graph coordinates are relative to the stile.
        int minX = -stile.STILE_WIDTH / 2 + (int) stile.transform.position.x;
        int minY = -stile.STILE_WIDTH / 2 + (int)stile.transform.position.y;
        int maxX = stile.STILE_WIDTH / 2 + (int)stile.transform.position.x;
        int maxY = stile.STILE_WIDTH / 2 + (int)stile.transform.position.y;

        ContactFilter2D filter = GetFilterWithoutTriggers(~LayerMask.GetMask("Ignore Raycast", "SlideableArea", "Player", "Rat"));
        RaycastHit2D[] hits = new RaycastHit2D[1];
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                int hit = Physics2D.CircleCast(pos, 0.5f, Vector2.up, filter, hits, 0f);
                if (hit == 0)
                {
                    result.Add(pos - new Vector2Int((int) stile.transform.position.x, (int) stile.transform.position.y));
                }
            }
        }

        return result;
    }

    //L: Calculates the shortest path from start to end.
    //BStar = Boomo Star
    public bool GetPathFromToBStar(Vector2Int start, Vector2Int end, out List<Vector2Int> path, bool includeStart = false)
    {
        path = new List<Vector2Int>();
        if (!validPts.Contains(start) || !validPts.Contains(end))
        {
            Debug.LogError("A* Algorithm tried to calculate a path for nonexistant nodes!");
            return false;
        }

        var visited = new HashSet<Vector2Int>();
        //The cost of travelling from start to a given node
        var costs = new Dictionary<Vector2Int, int>();
        //The previous node in the shortest path from start to a given node (can backtrack to get path)
        var prevNode = new Dictionary<Vector2Int, Vector2Int>();
        //The priority queue that returns the lowest cost node
        var nodeQueue = new SimplePriorityQueue<Vector2Int, int>();

        //Initialze all values in the data structure
        foreach (Vector2Int node in validPts)
        {
            costs[node] = node.Equals(start) ? 0 : int.MaxValue;
            prevNode[node] = Vector2Int.zero;
        }
        nodeQueue.Enqueue(start, 0);

        while (nodeQueue.Count > 0)
        {
            var curr = nodeQueue.Dequeue();
            visited.Add(curr);

            List<Vector2Int> neighbors = GetMooreNeighbors(curr);

            for (int neighborI = 0; neighborI < neighbors.Count; neighborI++)
            {

                var neighbor = neighbors[neighborI];
                if (!visited.Contains(neighbor))
                {
                    //A* Heuristic: 
                    //G Cost = costs[curr] + GetCost(curr, neighbor) (Cost to get to this node plus the edge weight to the neighbor
                    //H Cost = GetCost(neighbor, end) (Distance from neighbor to end)
                    int newCost = costs[curr] + GetCost(curr, neighbor) + GetCost(neighbor, end);
                    if (newCost < costs[neighbor])
                    {
                        //Update the node's cost, and set it's path to come from the current node.
                        costs[neighbor] = newCost;
                        prevNode[neighbor] = curr;

                        nodeQueue.Enqueue(neighbor, newCost);
                    }
                }
            }
        }

        if (costs[end] < int.MaxValue)
        {
            //Backtrack to find the optimal path
            var curr = end;
            while (curr != start)
            {
                if (curr == null)
                {
                    Debug.LogError("A* Algorithm: A cost was found for this path even though the path is broken");
                    path.Clear();
                    return false;
                }
                path.Add(curr);
                curr = prevNode[curr];  //backtrack
            }

            if (includeStart)
            {
                path.Add(start);
            }
            path.Reverse();
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<Vector2Int> GetMooreNeighbors(Vector2Int curr)
    {
        var result = new List<Vector2Int>(8);   //8 neighbors max
        Vector2Int[] cardinalDirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Vector2Int[] diagDirs = { new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

        foreach (var dir in cardinalDirs)
        {
            if (validPts.Contains(curr+dir))
            {
                result.Add(curr+dir);
            }
        }

        foreach (var dir in diagDirs)
        {
            if (validPts.Contains(curr+dir) && validPts.Contains(curr + new Vector2Int(dir.x, 0)) && validPts.Contains(curr + new Vector2Int(0, dir.y)))
            {
                result.Add(curr+dir);
            }
        }

        return result;
    }

    private int GetCost(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(Mathf.RoundToInt(10 * Vector2Int.Distance(from, to)));
    }

    public void SetPathToDebug()
    {
        if (!GetPathFromToBStar(debugFrom, debugTo, out debugPath, true))
        {
            Debug.LogWarning("Debug positions set do not have a valid path");
        }
    }

    private ContactFilter2D GetFilterWithoutTriggers(int layerMask)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter = filter.NoFilter();
        filter.useTriggers = false;
        filter.useLayerMask = true;
        filter.layerMask = layerMask;

        return filter;
    }

    private void OnDrawGizmosSelected()
    {
        if (validPts != null)
        {
            foreach (Vector2Int pos in validPts)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(new Vector3(pos.x, pos.y, 0), 0.2f);
            }

            if (debugPath != null)
            {
                Gizmos.color = Color.yellow;

                for (int i = 0; i < debugPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(new Vector3(debugPath[i].x, debugPath[i].y, 0), new Vector3(debugPath[i + 1].x, debugPath[i + 1].y, 0));
                }
            }
        }
    }
}