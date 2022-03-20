using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STileNavigation : MonoBehaviour
{
    private Graph<PosNodeType> navGraph;
    [SerializeField]
    private STile stile;

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
        //The inefficient way to do this

        int minX = (int) stile.transform.position.x - stile.STILE_WIDTH / 2;
        int minY = (int) stile.transform.position.y - stile.STILE_WIDTH / 2;
        int maxX = (int) stile.transform.position.x + stile.STILE_WIDTH / 2;
        int maxY = (int) stile.transform.position.y + stile.STILE_WIDTH / 2;

        navGraph = new Graph<PosNodeType>();
        Debug.Log("Creating Graph");
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
                Vector2Int pointToCheck = pos + dir;
                GraphNode<PosNodeType> nodeToCheck = null;
                //This is dumb
                foreach (GraphNode<PosNodeType> node2 in navGraph.Nodes)
                {
                    if (node2.Value.Position == pointToCheck)
                    {
                        nodeToCheck = node2;
                    }
                }
                Debug.Log(nodeToCheck); 
                if (nodeToCheck != null)
                {
                    ContactFilter2D filter = GetRaycastFilter();
                    RaycastHit2D[] hits = new RaycastHit2D[1];  //We only care about the first hit.
                    int hit = Physics2D.Raycast(pos, dir, filter, hits, Vector2Int.Distance(pos, pointToCheck));
                    if (hit == 0)
                    {
                        Debug.Log("FoundValidEdge");
                        navGraph.AddDirectedEdge(node, nodeToCheck, node.Value.GetCostTo(nodeToCheck.Value));
                    }
                }
            }
        }
        Debug.Log(navGraph.Nodes.Count);

        //Get rid of nodes with no neighbor (i.e. the ones on the colliders)
        navGraph.PruneIsolatedNodes();

        Debug.Log(navGraph.Nodes.Count);
    }

    private void OnDrawGizmosSelected()
    {
        if (navGraph != null)
        {
            Debug.Log(navGraph.Nodes.Count);
            foreach (GraphNode<PosNodeType> node in navGraph.Nodes)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(new Vector3(node.Value.Position.x, node.Value.Position.y, 0), 0.2f);

                foreach (GraphNode<PosNodeType> neighbor in node.Neighbors)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(new Vector3(node.Value.Position.x, node.Value.Position.y, 0), new Vector3(neighbor.Value.Position.x, neighbor.Value.Position.y, 0));
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
