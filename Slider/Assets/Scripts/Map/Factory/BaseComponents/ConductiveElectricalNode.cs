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
            AddNeighbor(node);
        }
    }

    private void OnTriggerStay2D(Collision2D collision)
    {
        ConductiveElectricalNode node = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (node != null && !neighbors.Contains(node))
        {
            AddNeighbor(node);
        }
    }

    private void OnTriggerExit2D(Collision2D collision)
    {
        ConductiveElectricalNode node = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (node != null)
        {
            RemoveNeighbor(node);
        }
    }
}
