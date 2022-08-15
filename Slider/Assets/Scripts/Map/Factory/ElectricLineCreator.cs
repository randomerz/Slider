using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricLineCreator : MonoBehaviour
{
    [Header("ElectricLineCreator")]
    [SerializeField] private ElectricalNode eNode;
    [SerializeField] private Transform lineStart;
    [SerializeField] private Animator anim;

    //(Node that the pilon is connected to, instantiation of the effect)
    protected Dictionary<ConductiveElectricalNode, GameObject> electricalLines;

    protected HashSet<ConductiveElectricalNode> nodesConducting;

    private GameObject electricLinePrefab;

    private void Awake()
    {
        electricalLines = new Dictionary<ConductiveElectricalNode, GameObject>();
        nodesConducting = new HashSet<ConductiveElectricalNode>();
        electricLinePrefab = Resources.Load<GameObject>("ElectricLine");
    }

    private void OnEnable()
    {
        ConductiveElectricalNode.onAddNode += AddNode;
        ConductiveElectricalNode.onRemoveNode += RemoveNode;
        eNode.OnPoweredOn.AddListener(OnPoweredOn);
        eNode.OnPoweredOff.AddListener(OnPoweredOff);
    }

    private void OnDisable()
    {
        ConductiveElectricalNode.onAddNode -= AddNode;
        ConductiveElectricalNode.onRemoveNode -= RemoveNode;
        eNode.OnPoweredOn.RemoveListener(OnPoweredOn);
        eNode.OnPoweredOff.RemoveListener(OnPoweredOff);
    }

    public void AddNode(object sender, ConductiveElectricalNode.NodeEventArgs e)
    {
        if (e.from.gameObject == this.gameObject || e.to.gameObject == this.gameObject)
        {
            ConductiveElectricalNode other = e.from.gameObject == this.gameObject ? e.to : e.from;
            nodesConducting.Add(other);
            if (anim != null)
            {
                anim.SetBool("Conducting", true);
            }

            if (eNode.Powered)
            {
                CreateElectricLineEffect(other);
            }
        }

    }

    public void RemoveNode(object sender, ConductiveElectricalNode.NodeEventArgs e)
    {
        if (e.from.gameObject == this.gameObject || e.to.gameObject == this.gameObject)
        {
            ConductiveElectricalNode other = e.from.gameObject == this.gameObject ? e.to : e.from;
            nodesConducting.Remove(other);
            if (nodesConducting.Count == 0)
            {
                if (anim != null)
                {
                    anim.SetBool("Conducting", false);
                }
            }

            DeleteElectricalLineEffect(other);
        }
    }

    public void OnPoweredOn()
    {
        if (anim != null)
        {
            anim.SetBool("Powered", true);
        }

        foreach (ConductiveElectricalNode node in nodesConducting)
        {
            if (!electricalLines.ContainsKey(node))
            {
                CreateElectricLineEffect(node);
                if (anim != null)
                {
                    anim.SetBool("Conducting", true);
                }
            }
        }
    }

    public void OnPoweredOff()
    {
        if (anim != null)
        {
            anim.SetBool("Powered", false);
            anim.SetBool("Conducting", false);
        }

        ClearElectricalLines();
    }

    private void CreateElectricLineEffect(ConductiveElectricalNode other)
    {
        if (!electricalLines.ContainsKey(other)) {
            GameObject electricLineInstance = Instantiate(electricLinePrefab, lineStart.transform);
            LineRenderer lr = electricLineInstance.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, Vector3.zero);
            lr.SetPosition(1, other.transform.position-lineStart.transform.position);
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