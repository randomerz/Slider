using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBox : ElectricalInput
{
    public bool Powered
    {
        get;
        private set;
    }

    void Start()
    {
        SetPowered(false, true);
    }

    void SetPowered(bool input, bool initializer = false)
    {
        if (Powered != input || initializer)
        {
            Powered = input;
            Debug.Log("Powering: " + input);

            foreach (ElectricalNode node in outputNodes)
            {
                List<ElectricalNode> recStack = new List<ElectricalNode> ();
                node.PropagateSignal(Powered, recStack);
            }
        }
    }
}
