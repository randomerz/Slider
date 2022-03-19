using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STileNavigation : MonoBehaviour
{
    private Graph<PosNodeType> navGraph;

    private void BakeNavGrid(STile stile)
    {
        //The inefficient way to do this

        int minX = (int) stile.transform.position.x - stile.STILE_WIDTH / 2;
        int minY = (int) stile.transform.position.y - stile.STILE_WIDTH / 2;
        int maxX = (int) stile.transform.position.x + stile.STILE_WIDTH / 2;
        int maxY = (int) stile.transform.position.y + stile.STILE_WIDTH / 2;

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
                Vector2Int pointToCheck = pos + dir;
                GraphNode<PosNodeType> nodeToCheck = navGraph.GetNode(new PosNodeType(pos + dir));
                if (nodeToCheck != null)
                {
                    RaycastHit2D hit = Physics2D.Raycast(pos, dir, Vector2Int.Distance(pos, pointToCheck));
                    if (hit.collider == null)
                    {
                        navGraph.AddDirectedEdge(node, nodeToCheck, node.Value.GetCostTo(nodeToCheck.Value));
                    }
                }
            }
        }

        //Get rid of nodes with no neighbor (i.e. the ones on the colliders
        navGraph.PruneIsolatedNodes();
    }
}
