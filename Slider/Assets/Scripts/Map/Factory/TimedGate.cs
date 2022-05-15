using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedGate : ElectricalNode
{
    [SerializeField] private int numTurns;
    [SerializeField] private int numInputs;

    [SerializeField] private Animator anim;

    private HashSet<ElectricalNode> inputsPowered;

    private bool gateActive;    //Whether the timed gate is activated and displaying the countdown (Note: This is different from Powered)

    public override bool Powered
    {
        get
        {
            return inputsPowered != null && inputsPowered.Count >= numInputs;
        }
    }

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.IO;
        anim.SetBool("Active", false);
        anim.SetBool("Powered", false);

        inputsPowered = new HashSet<ElectricalNode>();
    }

    #region ElectricalNode Overrides

    protected override void PropagateSignal(bool value, ElectricalNode prev, HashSet<ElectricalNode> recStack, int numRefs = 1)
    {
        bool oldPowered = Powered;
        if (EvaluateNodeInput(value, prev, recStack, numRefs) && value && gateActive)
        {
            inputsPowered.Add(prev);

            if (Powered != oldPowered)
            {
                OnPowered?.Invoke(new OnPoweredArgs { powered = Powered });
            }
        }

    }
    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        //Once the timed gate is powered, it essentially acts as an input source to it's outputs, so we only care about when it is powered.

        if (e.powered)
        {
            anim.SetBool("Powered", true);
            PushSignalToOutput(true, new HashSet<ElectricalNode>(), 1);
        }
    }
    #endregion

    public void GateOn()
    {
        gateActive = true;
        anim.SetBool("Active", true);

        foreach (ElectricalNode input in powerPathPrevs)
        {
            //Add all the nodes that were already connected to the gate when it was turned on.
            inputsPowered.Add(input);
        }
    }

    public void GateOff()
    {
        gateActive = false;
        anim.SetBool("Active", false);

        if (!Powered)
        {
            //Player failed to power the inputs in time.
            inputsPowered.Clear();
        }
    }
}