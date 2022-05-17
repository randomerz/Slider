using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedGateDiode : PoweredLightNew
{
    [SerializeField] private TimedGate gate;


    private new void OnEnable()
    {
        base.OnEnable();
        gate.OnGateDeactivated.AddListener(GateDeactivatedHandler);
    }

    private new void OnDisable()
    {
        base.OnDisable();
        gate.OnGateDeactivated.RemoveListener(GateDeactivatedHandler);
    }


    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        if (e.powered && gate.GateActive)
        {
            swapper.TurnOn();
        }
    }

    public void GateDeactivatedHandler()
    {
        swapper.TurnOff();
    }
}
