using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductiveElectricalNode : ElectricalNode
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ConductiveElectricalNode other = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        STile stile = GetComponent<STile>();
        if (other != null && BothNodesNotMoving(other))
        {
            AddNeighbor(other);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        ConductiveElectricalNode other = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (other != null && !(neighbors.Contains(other) || other.neighbors.Contains(this)) && BothNodesNotMoving(other))
        {
            AddNeighbor(other);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ConductiveElectricalNode other = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (other != null)
        {
            RemoveNeighbor(other);
        }
    }

    private bool BothNodesNotMoving(ConductiveElectricalNode other)
    {
        STile thisStile = GetComponentInParent<STile>();
        STile otherStile = other.GetComponentInParent<STile>();

        bool thisStay = true;
        bool otherStay = true;
        if (thisStile != null)
        {
            thisStay = thisStile.GetMovingDirection().Equals(Vector2.zero);
        }
        if (otherStile != null)
        {
            otherStay = otherStile.GetMovingDirection().Equals(Vector2.zero);
        }
        return thisStay && otherStay;
    }

}
