using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WirePilon : ConductiveElectricalNode
{
    //Conductive Line Effects

    //(Node that the pilon is connected to, instantiation of the effect)
    protected Dictionary<ConductiveElectricalNode, GameObject> electricalLines;

    protected HashSet<ConductiveElectricalNode> nodesConducting;

    private GameObject electricLine;

    [SerializeField] private Transform lineStart;
    [SerializeField] private Animator anim;


    private new void Awake()
    {
        electricalLines = new Dictionary<ConductiveElectricalNode, GameObject>();
        nodesConducting = new HashSet<ConductiveElectricalNode>();
        electricLine = Resources.Load<GameObject>("ElectricLine");

        base.Awake();
    }

    public void AddConductingNode(ConductiveElectricalNode other)
    {
        nodesConducting.Add(other);
        anim.SetBool("Conducting", true);

        if (Powered)
        {
            CreateElectricLineEffect(other);
        }
    }

    public void RemoveConductingNode(ConductiveElectricalNode other)
    {
        nodesConducting.Remove(other);
        if (nodesConducting.Count == 0)
        {
            anim.SetBool("Conducting", false);
        }

        DeleteElectricalLineEffect(other);
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);

        anim.SetBool("Powered", e.powered);

        if (e.powered)
        {
            foreach (ElectricalNode node in neighbors)
            {
                var nodeCond = node as ConductiveElectricalNode;
                if (nodesConducting.Contains(nodeCond) && !electricalLines.ContainsKey(nodeCond))
                {
                    CreateElectricLineEffect(nodeCond);
                    anim.SetBool("Conducting", true);
                }
            }
        } else
        {
            anim.SetBool("Conducting", false);
            ClearElectricalLines();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Called This?");
        ConductiveElectricalNode other = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (other != null && BothNodesNotMoving(other))
        {
            if (AddNeighbor(other))
            {
                AddConductingNode(other);
            }
        }
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        OnTriggerEnter2D(collision);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        ConductiveElectricalNode other = collision.gameObject.GetComponentInParent<ConductiveElectricalNode>();
        if (other != null)
        {
            if (RemoveNeighbor(other))
            {
                RemoveConductingNode(other);
            }
        }
    }

    private void CreateElectricLineEffect(ConductiveElectricalNode other)
    {
        if (!electricalLines.ContainsKey(other)) {
            GameObject electricLineInstance = Instantiate(electricLine);
            LineRenderer lr = electricLineInstance.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, lineStart.position);
            lr.SetPosition(1, other.transform.position);
            electricalLines[other] = electricLineInstance;
        }
    }

    private void DeleteElectricalLineEffect(ConductiveElectricalNode other)
    {
        if (electricalLines != null && electricalLines.ContainsKey(other))
        {
            Destroy(electricalLines[other]);
            electricalLines.Remove(other);
        }
    }

    private void ClearElectricalLines()
    {
        foreach (GameObject electricalLine in electricalLines.Values) {
            Destroy(electricalLine);
        }

        electricalLines.Clear();
    }
}
