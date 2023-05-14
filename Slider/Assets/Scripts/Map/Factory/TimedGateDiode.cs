using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedGateDiode : ElectricalNode
{
    [Header("Timed Gate Diode")]
    [SerializeField] private TimedGate gate;
    [SerializeField] private SpriteSwapper swapper;
    [SerializeField] private BatteryProp batteryProp;


    private new void OnEnable()
    {
        base.OnEnable();
        gate.OnGateActivated.AddListener(GateActivatedHandler);
        gate.OnGateDeactivated.AddListener(GateDeactivatedHandler);
        batteryProp?.UpdateSprite();
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
            //GateActivatedHandler();
            swapper.TurnOn();
            batteryProp?.SetDiodeEnabled(true);
        }
    }

    public void GateActivatedHandler()
    {
        //Debug.Log($"Timed Gate for Diode {gameObject.name} activated.");
        batteryProp?.SetGateEnabled(true);

        //if (Powered)
        //{
        //    swapper.TurnOn();
        //    batteryProp.SetDiodeEnabled(true);
        //}

    }

    public void GateDeactivatedHandler()
    {
        // Debug.Log($"Timed Gate for Diode {gameObject.name} deactivated.");
        swapper.TurnOff();
        batteryProp?.SetGateEnabled(false);
        batteryProp?.SetDiodeEnabled(false);
    }
}
