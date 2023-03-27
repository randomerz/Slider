using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Priority_Queue;

public class WorldNavigation : MonoBehaviour
{
    [SerializeField] private Tilemap worldFloorTM;

    //We cache these in sets since there are a lot of points to consider.
    private HashSet<Vector2Int> validPtsWorld = new HashSet<Vector2Int>();
    private Dictionary<STile, HashSet<Vector2Int>> validPtsStiles = new Dictionary<STile, HashSet<Vector2Int>>();
    private Dictionary<Vector2Int, bool> validPtsStilesCache = new Dictionary<Vector2Int, bool>(); 
    // cleared when validPtsStiles is updated. we can consider making a dictionary of stile to hash if recomputing happens too often

    private STile[] stiles;

    private List<STile> handleStileBuffer = new List<STile>();

    [Header("Debug")]
    [SerializeField]
    private Vector2Int debugFrom;
    [SerializeField]
    private Vector2Int debugTo;

    private List<Vector2Int> debugPath;

    public static event System.EventHandler<System.EventArgs> OnValidPtsChanged;

    private void Awake()
    {
        //Unfortunately, due to script order and some other complications, the moss won't update the points until it 
        //changes for the first time. This doesn't affect the game tho, so it's not super important to fix (until it is).
        validPtsWorld = GetWorldValidPts();

        validPtsStiles = new Dictionary<STile, HashSet<Vector2Int>>();
        stiles = GetComponentsInChildren<STile>();
        foreach (STile stile in stiles)
        {
            if (stile.isTileActive)
            {
                validPtsStiles[stile] = GetSTileValidPts(stile);
            }
        }
    }

    private void OnEnable()
    {
        SGrid.OnSTileEnabled += HandleSTileEnabled;
        CaveMossManager.MossUpdated += HandleMossUpdated;
        SGridAnimator.OnSTileMoveEnd += HandleTileMovementFinished;
    }

    private void OnDisable()
    {
        SGrid.OnSTileEnabled -= HandleSTileEnabled;
        CaveMossManager.MossUpdated -= HandleMossUpdated;
    }

    private void Update() 
    {
        for (int i = 0; i < handleStileBuffer.Count; i++)
        {
            STile stile = handleStileBuffer[i];
            if (!stile.IsMoving())
            {
                validPtsStiles[stile] = GetSTileValidPts(stile);
                ClearStileValidPtsCache();
                handleStileBuffer.RemoveAt(i);
                i -= 1;
            }
        }

    }

    private void HandleTileMovementChecks()
    {
        // Clear cache if tiles are moving
        foreach (STile stile in stiles)
        {
            if (stile.IsMoving())
            {
                ClearStileValidPtsCache();
            }
        }
    }

    private void HandleSTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        validPtsStiles[e.stile] = GetSTileValidPts(e.stile);
        ClearStileValidPtsCache();

        OnValidPtsChanged?.Invoke(this, new System.EventArgs());
    }

    private void HandleMossUpdated(object sender, CaveMossManager.MossUpdatedArgs e)
    {
        if (e.stile == null)
        {
            validPtsWorld = GetWorldValidPts();
        } 
        else
        {
            if (!e.stile.IsMoving())
            {
                validPtsStiles[e.stile] = GetSTileValidPts(e.stile);
                ClearStileValidPtsCache();
            }
            else
            {
                if (!handleStileBuffer.Contains(e.stile))
                {
                    handleStileBuffer.Add(e.stile);
                }
            }
        }
        OnValidPtsChanged?.Invoke(this, new System.EventArgs());
    }

    private void HandleTileMovementFinished(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        ClearStileValidPtsCache();
    }

    private HashSet<Vector2Int> GetWorldValidPts()
    {
        var result = new HashSet<Vector2Int>();
        ContactFilter2D filter = GetFilterWithoutTriggers(~LayerMask.GetMask("Ignore Raycast", "Player", "Rat", "NPC"));
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

    private HashSet<Vector2Int> GetSTileValidPts(STile stile)
    {
        var result = new HashSet<Vector2Int>();
        
        if (!stile.isTileActive)
            return result;

        // Debug.Log("Getting STile valid points for stile " + stile.islandId);
        
        //Graph coordinates are relative to the stile.
        int minX = -stile.STILE_WIDTH / 2 ;
        int minY = -stile.STILE_WIDTH / 2;
        int maxX = stile.STILE_WIDTH / 2;
        int maxY = stile.STILE_WIDTH / 2;

        ContactFilter2D filter = GetFilterWithoutTriggers(~LayerMask.GetMask("Ignore Raycast", "Player", "Rat")); //, "NPC"));
        RaycastHit2D[] hits = new RaycastHit2D[1];
        CaveMossManager moss = stile.GetComponentInChildren<CaveMossManager>();
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int posRel = new Vector2Int(x, y);
                Vector2 posAbs = RelToAbsPos(posRel, stile);

                int hit = Physics2D.CircleCast(posAbs, 0.25f, Vector2.up, filter, hits, 0f);
                if (hit == 0)
                {
                    if (moss == null || moss.mossCollidersMap.GetColliderType((Vector3Int) TileUtil.WorldToTileCoords(posAbs)) == Tile.ColliderType.None)
                    {
                        result.Add(posRel);
                    }
                }
            }
        }
        return result;
    }

    private Vector2Int RelToAbsPosTile(Vector2Int pos, STile stile)
    {
        return pos + TileUtil.WorldToTileCoords(stile.transform.position);
    }

    private Vector2 RelToAbsPos(Vector2Int pos, STile stile)
    {
        return pos + new Vector2(stile.transform.position.x, stile.transform.position.y);
    }

    private Vector2Int AbsToRelPos(Vector2Int pos, STile stile)
    {
        return pos - TileUtil.WorldToTileCoords(stile.transform.position);
    }

    //Check if a point is in.
    public bool IsValidPt(Vector2Int pos)
    {
        return validPtsWorld.Contains(pos) || IsValidPtOnStile(pos);
    }

    public bool IsValidPtOnStile(Vector2Int pos)
    {
        if (!validPtsStilesCache.ContainsKey(pos))
        {
            validPtsStilesCache[pos] = CalcIsValidPtOnStile(pos);
        }
        
        return validPtsStilesCache[pos];
    }

    private bool CalcIsValidPtOnStile(Vector2Int pos)
    {
        // This is slow when we do it a lot of times! even with our hash tables :(
        foreach (STile stile in stiles)
        {
            if (validPtsStiles.ContainsKey(stile) && validPtsStiles[stile].Contains(AbsToRelPos(pos, stile)))
            {
                return true;
            }
        }

        return false;
    }

    private void ClearStileValidPtsCache()
    {
        validPtsStilesCache.Clear();
    }

    //Perform some arbitrary function on every point.
    private void ForEachValidPt(Action<Vector2Int> func)
    {
        ForEachValidPtOutsideStile(func);
        ForEachValidPtOnStile(func);
    }

    private void ForEachValidPtOutsideStile(Action<Vector2Int> func)
    {
        if (validPtsWorld != null)
        {
            foreach (Vector2Int pt in validPtsWorld)
            {
                func(pt);
            }
        }
    }

    private void ForEachValidPtOnStile(Action<Vector2Int> func)
    {
        foreach (STile stile in stiles)
        {
            if (validPtsStiles.ContainsKey(stile))
            {
                foreach (Vector2Int pt in validPtsStiles[stile])
                {
                    func(RelToAbsPosTile(pt, stile));
                }
            }
        }
    }

    //L: Calculates the shortest path from start to end.
    public bool GetPathFromToAStar(Vector2Int start, Vector2Int end, out List<Vector2Int> path, bool includeStart = false, Func<Vector2Int, Vector2Int, Vector2Int, int> costFunc = null)
    {
        if (costFunc == null)
        {
            costFunc = new Func<Vector2Int, Vector2Int, Vector2Int, int>(GetAStarCost);
        }

        path = new List<Vector2Int>();
        if (!IsValidPt(start))
        {
            //Check all the neighbors too in case this is an edge case.
            foreach (Vector2Int pt in GetMooreNeighbors(start))
            {
                if (!IsValidPt(pt))
                {
                    Debug.LogWarning($"Invalid Start: {start} This might be intentional (not an error).");
                    return false;
                }
            }
        }

        if (!IsValidPt(end))
        {
            Debug.LogWarning($"Invalid End: {end} This might be intentional (not an error).");
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
        ForEachValidPt((pt) =>
        {
            costs[pt] = int.MaxValue;
            prevNode[pt] = Vector2Int.zero;
        });

        costs[start] = 0;
        costs[end] = int.MaxValue;

        nodeQueue.Enqueue(start, 0);

        while (nodeQueue.Count > 0 && costs[end] >= int.MaxValue)
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
                    //G Cost = costs[curr] + GetCost(curr, neighbor) (Cost to get to this node plus the edge weight to the neighbor)
                    //H Cost = GetCost(neighbor, end) (Distance from neighbor to end)

                    //Need to check against overflow.
                    int newCost;
                    int edgeCost = costFunc(curr, neighbor, end);

                    if (!costs.ContainsKey(curr))
                    {
                        Debug.LogError($"Error Calculating Path: Tried to iterate on position {curr} that was not initialized in costs.");
                    }
                    if (costs[curr] == int.MaxValue || edgeCost == int.MaxValue)
                    {
                        newCost = int.MaxValue;
                    } else
                    {
                        newCost = costs[curr] + edgeCost;
                    }

                    if (!costs.ContainsKey(neighbor))
                    {
                        costs[neighbor] = int.MaxValue;
                    }

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
            if (IsValidPt(curr+dir))
            {
                result.Add(curr+dir);
            }
        }

        foreach (var dir in diagDirs)
        {
            if (IsValidPt(curr+dir) && IsValidPt(curr + new Vector2Int(dir.x, 0)) && IsValidPt(curr + new Vector2Int(0, dir.y)))
            {
                result.Add(curr+dir);
            }
        }

        return result;
    }

    public static int GetAStarCost(Vector2Int curr, Vector2Int neighbor, Vector2Int end)
    {
        return GetDistanceCost(curr, neighbor) + GetDistanceCost(neighbor, end);
    }

    public static int GetDistanceCost(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(Mathf.RoundToInt(10 * Vector2Int.Distance(from, to)));
    }

    public void SetPathToDebug()
    {
        if (!GetPathFromToAStar(debugFrom, debugTo, out debugPath, true))
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
        if (stiles == null)
        {
            stiles = GetComponentsInChildren<STile>();
        }

        if (validPtsWorld != null && stiles.Length > 0)
        {
            ForEachValidPtOutsideStile ((pos) => {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(new Vector3(pos.x, pos.y, 0), 0.2f);
            });

            ForEachValidPtOnStile((pos) => {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(new Vector3(pos.x, pos.y, 0), 0.2f);
            });

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