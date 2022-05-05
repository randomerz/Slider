using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductiveElectricalNode : ElectricalNode
{
    private void OnTriggerEnter2D(Collision2D collision)
    {
        ConductiveElectricalNode node = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (node != null)
        {
            AddConnection(node);
        }
    }

    private void OnTriggerExit2D(Collision2D collision)
    {
        ConductiveElectricalNode node = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (node != null)
        {
            RemoveConnection(node);
        }
    }

    private void OnTriggerStay2D(Collision2D collision)
    {
        ConductiveElectricalNode node = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (node != null && !neighbors.Contains(node))
        {
            AddConnection(node);
        }
    }

    private void AddConnection(ConductiveElectricalNode other)
    {
        //Directed edges are easy since can just propagate along the direction of the edge.
        if (nodeType == NodeType.INPUT || other.nodeType == NodeType.OUTPUT)
        {
            //Update this reference table and AddNeighbor
            AddNeighbor(other);
        }

        if (other.nodeType != NodeType.INPUT)
        {

        }
        if (!neighbors.Contains(other))
        {
            //Handle the connection points
            int totalRefCount = this.powerRefCount + other.powerRefCount;

            //Propagate both values towards other neighbors.
        }
    }

    private void RemoveConnection(ConductiveElectricalNode other)
    {

    }
}
