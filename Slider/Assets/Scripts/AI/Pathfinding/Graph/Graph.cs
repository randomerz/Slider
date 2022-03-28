using System.Collections.Generic;

using UnityEngine;
using Priority_Queue;   //L: This is from a 3rd party: https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/releases/tag/v5.1.0

public class Graph<T>
{
    private Dictionary<T, GraphNode<T>> nodeDict;

    public Dictionary<T, GraphNode<T>>.ValueCollection Nodes
    {
        get
        {
            return nodeDict.Values;
        }
    }

    public int Count
    {
        get { return nodeDict.Count; }
    }

    public Graph() : this(null) { }

    public Graph(HashSet<GraphNode<T>> nodeSet)
    {
        this.nodeDict = new Dictionary<T, GraphNode<T>>();
        if (nodeSet != null)
        {
            foreach (GraphNode<T> node in nodeSet)
            {
                this.nodeDict[node.Value] = node;
            }
        }
    }

    public void AddNode(GraphNode<T> node)
    {
        // adds a node to the graph
        nodeDict[node.Value] = node;
    }

    public void AddNode(T value)
    {
        // adds a node to the graph with value given
        nodeDict[value] = new GraphNode<T>(value);
    }

    public void AddDirectedEdge(GraphNode<T> from, GraphNode<T> to, int cost = 1)
    {
        from.Neighbors.Add(to);
        from.Costs.Add(cost);
    }

    public void AddUndirectedEdge(GraphNode<T> from, GraphNode<T> to, int cost = 1)
    {
        from.Neighbors.Add(to);
        from.Costs.Add(cost);

        to.Neighbors.Add(from);
        to.Costs.Add(cost);
    }

    public void PruneIsolatedNodes()
    {
        var isolatedNodes = new HashSet<T>();
        foreach (T node in nodeDict.Keys)
        {
            if (nodeDict[node].Neighbors.Count == 0)
            {
                isolatedNodes.Add(node);
            }
        }

        foreach (T node in isolatedNodes)
        {
            nodeDict.Remove(node);
        }
    }

    public GraphNode<T> GetNode(T value)
    {
        return Contains(value) ? nodeDict[value] : null;
    }

    public bool Contains(T value)
    {
        return nodeDict.ContainsKey(value);
    }

    public bool Remove(T value)
    {
        // first remove the node from the nodese
        if (!nodeDict.ContainsKey(value))
        {
            // node wasn't found
            return false;
        }

        GraphNode<T> nodeToRemove = nodeDict[value];

        // otherwise, the node was found
        nodeDict.Remove(value);

        // enumerate through each node in the nodeSet, removing edges to this node
        foreach (GraphNode<T> gnode in Nodes)
        {
            int index = gnode.Neighbors.IndexOf(nodeToRemove);
            if (index != -1)
            {
                // remove the reference to the node and associated cost
                gnode.Neighbors.RemoveAt(index);
                gnode.Costs.RemoveAt(index);
            }
        }

        return true;
    }

    //L: Calculates the shortest path from start to end.
    public static bool AStar(Graph<PosNodeType> graph, PosNodeType start, PosNodeType end, out List<Vector2Int> path, bool includeStart = true)
    {
        path = new List<Vector2Int>();
        if (start == null || end == null || !graph.Contains(start) || !graph.Contains(end))
        {
            Debug.LogError("A* Algorithm tried to calculate a path for nonexistant nodes!");
            return false;
        }

        var nodeStart = graph.GetNode(start);
        var nodeEnd = graph.GetNode(end);

        var visited = new HashSet<GraphNode<PosNodeType>>();
        //The cost of travelling from start to a given node
        var costs = new Dictionary<GraphNode<PosNodeType>, int>();
        //The previous node in the shortest path from start to a given node (can backtrack to get path)
        var prevNode = new Dictionary<GraphNode<PosNodeType>, GraphNode<PosNodeType>>();
        //The priority queue that returns the lowest cost node
        var nodeQueue = new SimplePriorityQueue<GraphNode<PosNodeType>, int>();

        //Initialze all values in the data structure
        foreach (GraphNode<PosNodeType> node in graph.Nodes)
        {
            costs[node] = node.Value.Equals(start) ? 0 : int.MaxValue;
            prevNode[node] = null;
        }
        nodeQueue.Enqueue(nodeStart, 0);

        while (nodeQueue.Count > 0)
        {
            var curr = nodeQueue.Dequeue();
            visited.Add(curr);

            for(int neighborI=0; neighborI < curr.Neighbors.Count; neighborI++) {

                var neighbor = curr.Neighbors[neighborI];
                if (!visited.Contains(neighbor))
                {
                    //A* Heuristic: 
                    //G Cost = costs[curr] + curr.Costs[i] (Cost to get to this node plus the edge weight to the neighbor
                    //H Cost = Distance from neighbor to end
                    int newCost = costs[curr] + curr.Costs[neighborI] + neighbor.Value.GetCostTo(end);
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

        if (costs[nodeEnd] < int.MaxValue)
        {
            //Backtrack to find the optimal path
            var curr = nodeEnd;
            while (curr != nodeStart)
            {
                if (curr == null)
                {
                    Debug.LogError("A* Algorithm: A cost was found for this path even though the path is broken");
                    path.Clear();
                    return false;
                }
                path.Add(curr.Value.Position);
                curr = prevNode[curr];  //backtrack
            }

            if (includeStart)
            {
                path.Add(nodeStart.Value.Position);
            }
            path.Reverse();
            return true;
        } else
        {
            return false;
        }
    }
}
