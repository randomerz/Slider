using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBox : ElectricalNode
{

    private void Awake()
    {
        nodeType = NodeType.INPUT;
    }

    void Start()
    {
        SetPowered(false, true);
    }

    void SetPowered(bool input, bool initializer = false)
    {
        if (Powered != input || initializer)
        {
            powerRefCount = input ? 1 : 0;
            Debug.Log("Powering: " + input);

            foreach (ElectricalNode node in neighbors)
            {
                List<ElectricalNode> recStack = new List<ElectricalNode> ();
                node.PropagateSignal(Powered, recStack);
            }
        }
    }
}
