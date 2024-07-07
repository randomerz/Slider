using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductiveElectricalNode : ElectricalNode
{
    [Header("Conductive Electrical Node")]
    [SerializeField] private bool isConductiveItem;
    [SerializeField] private bool isConductiveTerminus;
    [SerializeField] private bool ignoreNotMovingCheck;

    private const string DISTANCE_BASED_ELECTRICITY = "AmbienceDistToElectricity";
    private static float thisFrameDistToElectricity = float.MaxValue;
    private static bool thisFrameUpdatedFmod = false;
    private static bool isPlayingAmbience = false;

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

    protected override void Awake()
    {
        base.Awake();

        thisFrameDistToElectricity = float.MaxValue;

        if (!isPlayingAmbience)
        {
            isPlayingAmbience = true;
            AudioManager.PlayAmbience("Electricity Ambience");
        }
    }

    private void OnDestroy()
    {
        if (isPlayingAmbience)
        {
            isPlayingAmbience = false;
            AudioManager.StopAmbience("Electricity Ambience");
        }
    }

    protected override void Update()
    {
        base.Update();

        thisFrameUpdatedFmod = false;

        if ((isConductiveTerminus || isConductiveItem) && Powered)
        {
            float distToPlayer = Vector3.Distance(transform.position, Player.GetPosition());
            thisFrameDistToElectricity = Mathf.Min(distToPlayer, thisFrameDistToElectricity);
        }
    }

    private void LateUpdate()
    {
        if (!thisFrameUpdatedFmod)
        {
            AudioManager.SetGlobalParameter(DISTANCE_BASED_ELECTRICITY, thisFrameDistToElectricity);
            thisFrameUpdatedFmod = true;
            thisFrameDistToElectricity = float.MaxValue;
        }
    }

    private void OnValidate()
    {
        if (isConductiveItem && isConductiveTerminus)
        {
            Debug.LogWarning("Node cannot be both conductive item and terminus. This will result in unexpected behaviour.");
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        CheckAddConnection(other);
    }

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        CheckAddConnection(other);
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        CheckRemoveConnection(other);
    }

    private void CheckAddConnection(Collider2D other)
    {
        ConductiveElectricalNode otherNode = other.gameObject.GetComponentInParent<ConductiveElectricalNode>();

        if (otherNode != null)
        {
            if (BothNodesMovingOkay(otherNode) && ConductiveItemCheck(otherNode))
            {
                if (AddConnection(otherNode))
                {
                    if (Powered)
                    {
                        AudioManager.Play("New Electricity Connection", transform);
                    }
                    onAddNode?.Invoke(this, new NodeEventArgs(this, otherNode));
                }
            }
        }
    }

    private void CheckRemoveConnection(Collider2D other)
    {
        ConductiveElectricalNode otherNode = other.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (otherNode != null)
        {
            if (RemoveConnection(otherNode))
            {
                onRemoveNode?.Invoke(this, new NodeEventArgs(this, otherNode));
            }
        }
    }

    private bool ConductiveItemCheck(ConductiveElectricalNode other)
    {
        bool bothConductiveItems = isConductiveItem && other.isConductiveItem;
        bool itemTerminusCheck = isConductiveItem == other.isConductiveTerminus && isConductiveTerminus == other.isConductiveItem;
        return !bothConductiveItems && itemTerminusCheck;
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
    }

    protected bool BothNodesMovingOkay(ConductiveElectricalNode other)
    {
        if (ignoreNotMovingCheck)
        {
            return true;
        }

        STile thisStile = GetComponentInParent<STile>();
        STile otherStile = other.GetComponentInParent<STile>();

        if (thisStile == otherStile)
        {
            return true;
        }

        // For cross stile checks
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

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.yellow;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 4);
        foreach (Collider2D h in hits)
        {
            ConductiveElectricalNode node = h.GetComponentInParent<ConductiveElectricalNode>();
            if (node != null)
            {
                Gizmos.DrawLine(transform.position, node.transform.position);
            }
        }
    }
}
