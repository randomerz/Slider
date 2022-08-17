using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductiveElectricalNode : ElectricalNode
{
    [Header("Conductive Electrical Node")]
    [SerializeField] private bool isConductiveItem;
    [SerializeField] private bool ignoreNotMovingCheck;

    public struct NodeEventArgs
    {
        public ConductiveElectricalNode from;
        public ConductiveElectricalNode to;

        public NodeEventArgs(ConductiveElectricalNode from, ConductiveElectricalNode to)
        {
            this.from = from;
            this.to = to;
        }
    }

    public static event System.EventHandler<NodeEventArgs> onAddNode;
    public static event System.EventHandler<NodeEventArgs> onRemoveNode;

    private new void Awake()
    {
        base.Awake();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        ConductiveElectricalNode other = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();

        //Disallow connections btw 2 conductive items (for now)
        if (other != null)
        {
            bool bothConductiveItems = isConductiveItem && other.isConductiveItem;
            if (BothNodesNotMoving(other) && !bothConductiveItems)
            {
                if (AddNeighbor(other))
                {
                    onAddNode?.Invoke(this, new NodeEventArgs(this, other));
                }
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
            if (RemoveNeighbor(other))
            {
                onRemoveNode?.Invoke(this, new NodeEventArgs(this, other));
            }
        }
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
    }

    protected bool BothNodesNotMoving(ConductiveElectricalNode other)
    {
        if (ignoreNotMovingCheck)
        {
            return true;
        }

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
