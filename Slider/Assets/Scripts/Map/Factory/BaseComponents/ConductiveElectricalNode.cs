using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductiveElectricalNode : ElectricalNode
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ConductiveElectricalNode node = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (node != null)
        {
            Debug.Log($"{gameObject.name} entering {collision.gameObject.name}");
            AddNeighbor(node);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        ConductiveElectricalNode node = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (node != null && !(neighbors.Contains(node) || node.neighbors.Contains(this)))
        {
            Debug.Log($"{gameObject.name} colliding with {collision.gameObject.name}");
            AddNeighbor(node);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ConductiveElectricalNode node = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (node != null)
        {
            Debug.Log($"{gameObject.name} exiting {collision.gameObject.name}");
            RemoveNeighbor(node);
        }
    }
}
