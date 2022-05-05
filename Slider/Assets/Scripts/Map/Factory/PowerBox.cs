using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBox : ConductiveElectricalNode
{

    private void Awake()
    {
        base.Awake();
        nodeType = NodeType.INPUT;
    }

    void Start()
    {
        SetPowered(true, true);
    }

    void SetPowered(bool input, bool initializer = false)
    {
        if (Powered != input || initializer)
        {
            powerRefs = input ? 1 : 0;
            Debug.Log("Powering: " + input);

            foreach (ElectricalNode node in neighbors)
            {
                HashSet<ElectricalNode> recStack = new HashSet<ElectricalNode>() { this };
                node.PropagateSignal(input, this, recStack);
            }
        }
    }
}
