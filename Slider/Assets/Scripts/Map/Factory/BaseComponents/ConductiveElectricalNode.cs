using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductiveElectricalNode : ElectricalNode
{
    [SerializeField]
    private bool isConductiveItem;

    private new void Awake()
    {
        base.Awake();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        ConductiveElectricalNode other = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();

        //Disallow connections btw 2 conductive items (for now)
        if (other != null && BothNodesNotMoving(other) && !(isConductiveItem && other.isConductiveItem))
        {
            if (AddNeighbor(other) && other is WirePilon)
            {
                (other as WirePilon).AddConductingNode(this);
            }
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        OnTriggerEnter2D(collision);
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        ConductiveElectricalNode other = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (other != null)
        {
            if (RemoveNeighbor(other) && other is WirePilon)
            {
                (other as WirePilon).RemoveConductingNode(this);
            }
        }
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
    }

    protected bool BothNodesNotMoving(ConductiveElectricalNode other)
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
