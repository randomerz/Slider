using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedGateDiode : ElectricalNode
{
    [Header("Timed Gate Diode")]
    [SerializeField] private TimedGate gate;
    [SerializeField] private SpriteSwapper swapper;


    private new void OnEnable()
    {
        base.OnEnable();
        gate.OnGateActivated.AddListener(GateActivatedHandler);
        gate.OnGateDeactivated.AddListener(GateDeactivatedHandler);
    }

    private new void OnDisable()
    {
        base.OnDisable();
        gate.OnGateActivated.RemoveListener(GateActivatedHandler);
        gate.OnGateDeactivated.RemoveListener(GateDeactivatedHandler);
    }


    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
        if (e.powered && gate.GateActive)
        {
            swapper.TurnOn();
        }
    }

    public void GateActivatedHandler()
    {
        // Debug.Log($"Timed Gate for Diode {gameObject.name} activated.");
        if (Powered)
        {
            swapper.TurnOn();
        }

    }

    public void GateDeactivatedHandler()
    {
        // Debug.Log($"Timed Gate for Diode {gameObject.name} deactivated.");
        swapper.TurnOff();
    }
}
