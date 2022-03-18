using System;
using System.Collections.Generic;

//T is the "payload" perse (the information held by the node)
public class GraphNode<T>
{
    private T value;

    //The Adjacency list representation for this node.
    private List<GraphNode<T>> neighbors;
    private List<int> costs;

    public T Value
    {
        get 
        { 
            return value; 
        }
    }

    public List<GraphNode<T>> Neighbors
    {
        get
        {
            if (neighbors == null)
            {
                neighbors = new List<GraphNode<T>>();
            }

            return neighbors;
        }
    }

    public List<int> Costs
    {
        get
        {
            if (costs == null)
            {
                costs = new List<int>();
            }

            return costs;
        }
    }

    public GraphNode(T value) : this(value, new List<GraphNode<T>>())
    {
    }

    public GraphNode(T value, List<GraphNode<T>> neighbors)
    {
        this.value = value;
        this.neighbors = neighbors;
    }

    //Two nodes with the same value are the same node
    public override bool Equals(object obj)
    {
        return ((GraphNode<T>)obj).value.Equals(this.value);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}


