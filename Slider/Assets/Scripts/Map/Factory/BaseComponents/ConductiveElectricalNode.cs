using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductiveElectricalNode : ElectricalNode
{
    //Conductive Line Effects
    [SerializeField] protected GameObject electricalLinePrefab;

    [SerializeField] public bool isConductiveObject;

    protected Dictionary<ElectricalNode, GameObject> electricalLines;

    protected Dictionary<ElectricalNode, Vector2> conductionPoints;

    private new void Awake()
    {
        base.Awake();

        conductionPoints = new Dictionary<ElectricalNode,Vector2>();
        electricalLines = new Dictionary<ElectricalNode, GameObject>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ConductiveElectricalNode other = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();

        if (other != null && BothNodesNotMoving(other))
        {
            if (conductionPoints == null)
            {
                conductionPoints = new Dictionary<ElectricalNode, Vector2>();
            }
            if (other.conductionPoints == null)
            {
                other.conductionPoints = new Dictionary<ElectricalNode, Vector2>();
            }
            if (!(neighbors.Contains(other) || other.neighbors.Contains(this)))
            {
                Collider2D thisCol = ClosestColliderToPoint(collision.gameObject.transform.position);
                if (thisCol != null)
                {
                    conductionPoints[other] = thisCol.gameObject.transform.position;
                    other.conductionPoints[this] = collision.gameObject.transform.position;
                }

                //Debug.Log(thisCol.gameObject.transform.position);
                //Debug.Log(collision.gameObject.transform.position);
            }

            if (AddNeighbor(other))
            {
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnTriggerEnter2D(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ConductiveElectricalNode other = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (other != null)
        {
            if (RemoveNeighbor(other))
            {
                conductionPoints.Remove(other);
                other.conductionPoints.Remove(this);
            }
        }
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
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

    private Collider2D ClosestColliderToPoint(Vector2 otherPos)
    {
        Collider2D[] thisColliders = GetComponentsInChildren<Collider2D>();
        //Vector2 minPt = thisColliders[0].ClosestPoint(otherPos);
        float minDist = float.MaxValue;
        Collider2D minCol = null;
        foreach (Collider2D col in thisColliders)
        {
            Vector2 pt = col.ClosestPoint(otherPos);
            float dist = Vector2.Distance(otherPos, pt);
            if (dist < minDist)
            {
                minDist = dist;
                minCol = col;
            }
        }

        return minCol;
    }

    public virtual void CreateElectricalLineEffect(ConductiveElectricalNode prev)
    {
        if (electricalLines == null)
        {
            electricalLines = new Dictionary<ElectricalNode, GameObject>();
        }
        if (this.conductionPoints == null)
        {
            this.conductionPoints = new Dictionary<ElectricalNode, Vector2>();
        }
        if (prev.conductionPoints == null)
        {
            prev.conductionPoints = new Dictionary<ElectricalNode, Vector2>();
        }
        if (prev.conductionPoints[this] == null || this.conductionPoints[prev] == null)
        {
            Debug.LogWarning("Tried to create electrical lines without conduction points.");
            return;
        }

        GameObject electricalLineInstance = Instantiate(electricalLinePrefab);
        LineRenderer lr = electricalLineInstance.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, prev.conductionPoints[this]);
        lr.SetPosition(1, this.conductionPoints[prev]);
        electricalLines[prev] = electricalLineInstance;

        Wire prevWire = prev as Wire;
        Wire thisWire = this as Wire;
        if (prevWire != null)
        {
            prevWire.ChangeToAlt((Vector3Int) TileUtil.WorldToTileCoords(prev.conductionPoints[this]), true);
        }
        if (thisWire != null)
        {
            thisWire.ChangeToAlt((Vector3Int)TileUtil.WorldToTileCoords(this.conductionPoints[prev]), true);
        }
    }

    public virtual void DeleteElectricalLineEffect(ConductiveElectricalNode other)
    {
        if (electricalLines != null && electricalLines.ContainsKey(other))
        {
            Destroy(electricalLines[other]);
            electricalLines.Remove(other);

            Wire otherWire = other as Wire;
            Wire thisWire = this as Wire;
            if (otherWire != null)
            {
                otherWire.ChangeToAlt((Vector3Int)TileUtil.WorldToTileCoords(other.conductionPoints[this]), false);
            }
            if (thisWire != null)
            {
                thisWire.ChangeToAlt((Vector3Int)TileUtil.WorldToTileCoords(this.conductionPoints[other]), false);
            }
        }
    }
}
