using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//THIS CLASS IS NO LONGER USED (See WorldNavigation.cs)
/*
public class STileNavigation : MonoBehaviour
{
    public float agentRadius;

    private Graph<PosNodeType> navGraph;

    [SerializeField]
    private STile stile;

    [Header("Debug")]
    [SerializeField]
    private Vector2Int debugFrom;
    [SerializeField]
    private Vector2Int debugTo;

    private List<Vector2Int> debugPath;

    private void Awake()
    {
        stile = GetComponent<STile>();
    }

    private void OnEnable()
    {
        SGrid.OnSTileEnabled += OnTileEnabledHandler;
    }

    private void OnDisable()
    {
        SGrid.OnSTileEnabled -= OnTileEnabledHandler;
    }

    private void OnTileEnabledHandler(object sender, SGrid.OnSTileEnabledArgs e)
    {
        //Bake the nav grid once the tile gets enabled (so the colliders are set properly)
        if (e.stile == stile)
        {
            BakeNavGraph();
        }
    }

    public void BakeNavGraph()
    {
        //Graph coordinates are relative to the stile.
        int minX = - stile.STILE_WIDTH / 2;
        int minY = - stile.STILE_WIDTH / 2;
        int maxX = stile.STILE_WIDTH / 2;
        int maxY = stile.STILE_WIDTH / 2;

        navGraph = new Graph<PosNodeType>();
        //Populate with nodes for all tiles in the stile (17*17)
        for (int x=minX; x<=maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                navGraph.AddNode(new PosNodeType(new Vector2Int(x, y)));
            }
        }

        foreach (GraphNode<PosNodeType> node in navGraph.Nodes)
        {
            Vector2Int pos = node.Value.Position;
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right, 
                                      new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };
            foreach (var dir in dirs)
            {
                CheckAndAddEdge(node, dir);
            }
        }

        //Get rid of nodes with no neighbor (i.e. the ones on the colliders)
        navGraph.PruneIsolatedNodes();
    }

    private void CheckAndAddEdge(GraphNode<PosNodeType> node, Vector2Int dir)
    {
        Vector2Int pos = node.Value.Position;
        Vector2Int pointToCheck = pos + dir;

        //Go through all the nodes to find the neighboring position (This part is dumb)
        GraphNode<PosNodeType> nodeToCheck = null;
        foreach (GraphNode<PosNodeType> node2 in navGraph.Nodes)
        {
            if (node2.Value.Position == pointToCheck)
            {
                nodeToCheck = node2;
            }
        }

        if (nodeToCheck != null)
        {
            ContactFilter2D filter = GetRaycastFilter();
            RaycastHit2D[] hits = new RaycastHit2D[1];  //We only care about the first hit.
            int hit = Physics2D.CircleCast(pos + (Vector2) stile.transform.position, agentRadius, dir, filter, hits, Vector2Int.Distance(pos, pointToCheck));
            if (hit == 0)
            {
                navGraph.AddDirectedEdge(node, nodeToCheck, node.Value.GetCostTo(nodeToCheck.Value));
            }
        }
    }
*/

    /**
     * Return whether a valid path was found
     * 
     * from and to are world positions
     */
/*
    public List<Vector2Int> GetPathFromToHard(Vector2Int from, Vector2Int to)
    {
        Vector2Int stilePosAsInt = new Vector2Int((int) stile.transform.position.x, (int) stile.transform.position.y);
        List<Vector2Int> path = GetPathFromToRelative(from - stilePosAsInt, to - stilePosAsInt);
        Debug.Log(from - stilePosAsInt);
        Debug.Log(to - stilePosAsInt);
        if (path != null)
        {
            for(int i=0; i<path.Count; i++)
            {
                path[i] = new Vector2Int(path[i].x + stilePosAsInt.x, path[i].y + stilePosAsInt.y);
            }
        }
        return path;
    }

    //positions relative to the position of the stile (How the nodes are structured originally)
    public List<Vector2Int> GetPathFromToRelative(Vector2Int from, Vector2Int to)
    {
        PosNodeType fromNode = null;
        PosNodeType toNode = null;
        foreach (var node in navGraph.Nodes)
        {
            if (node.Value.Position == from)
            {
                fromNode = node.Value;
            }

            if (node.Value.Position == to)
            {
                toNode = node.Value;
            }
        }

        if (fromNode == null || toNode == null)
        {
            Debug.LogError("Attempted to use STileNavigation to calculate path between positions out of bounds of the STile");
            return null;
        }

        List<Vector2Int> path = new List<Vector2Int>();
        if (Graph<PosNodeType>.AStar(navGraph, fromNode, toNode, out path, false))
        {
            return path;
        }
        else
        {
            return null;
        }
    }

    public void SetPathToDebug()
    {
        debugPath = GetPathFromToRelative(debugFrom, debugTo);
        if (debugPath == null)
        {
            Debug.LogWarning("Debug positions set do not have a valid path");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (navGraph != null)
        {
            foreach (GraphNode<PosNodeType> node in navGraph.Nodes)
            {
                float x = node.Value.Position.x + stile.transform.position.x;
                float y = node.Value.Position.y + stile.transform.position.y;
                if (debugPath != null && debugPath.Contains(node.Value.Position))
                {
                    Gizmos.color = Color.green;
                } else
                {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawSphere(new Vector3(x, y, 0), 0.2f);

                foreach (GraphNode<PosNodeType> neighbor in node.Neighbors)
                {
                    float neighX = neighbor.Value.Position.x + stile.transform.position.x;
                    float neighY = neighbor.Value.Position.y + stile.transform.position.y;
                    if (debugPath != null && debugPath.Contains(node.Value.Position) && debugPath.Contains(neighbor.Value.Position))
                    {
                        Gizmos.color = Color.yellow;
                    } else
                    {
                        Gizmos.color = Color.blue;
                    }
                    Gizmos.DrawLine(new Vector3(x, y, 0), new Vector3(neighX, neighY, 0));
                }
            }
        }
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
}
*/