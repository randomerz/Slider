using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ElectricLineCreator : MonoBehaviour
{
    [Header("ElectricLineCreator")]
    [SerializeField] private ElectricalNode eNode;
    [SerializeField] private Transform lineStart;
    [SerializeField] private Animator anim;

    public UnityEvent OnConductingOn;
    public UnityEvent OnConductingOff;

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

            bool powerFlowsForward = eNode.Powered && eNode.ValidDirectionGoingTo(other);
            bool powerFlowsReverse = other.Powered && other.ValidDirectionGoingTo(eNode);
            if (powerFlowsForward || powerFlowsReverse)
            {
                if (anim != null)
                {
                    anim.SetBool("Conducting", true);
                }
                CreateElectricLineEffect(other);
                OnConductingOn?.Invoke();
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
                OnConductingOff?.Invoke();
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
                OnConductingOn?.Invoke();
                if (anim != null)
                {
                    anim.SetBool("Conducting", true);
                }
                CreateElectricLineEffect(node);
            }
        }
    }

    public void OnPoweredOff()
    {
        OnConductingOff?.Invoke();
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