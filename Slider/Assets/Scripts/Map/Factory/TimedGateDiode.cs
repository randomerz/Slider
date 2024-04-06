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
        
        // GateActive means the countdown is happening, gate.Powered is a bit weirder but 
        // is hopefully true when an area is loaded
        if (e.powered && (gate.GateActive || gate.Powered))
        {
            //GateActivatedHandler();
            swapper.TurnOn();
            batteryProp?.SetDiodeEnabled(true);
        }
    }

    public void GateActivatedHandler()
    {
        //Debug.Log($"Timed Gate for Diode {gameObject.name} activated.");

        // In case it started powered
        if (Powered)
        {
            swapper.TurnOn();
            batteryProp.SetDiodeEnabled(true);
            batteryProp?.SetGateEnabled(true);
        }
    }

    public void GateDeactivatedHandler()
    {
        // Debug.Log($"Timed Gate for Diode {gameObject.name} deactivated.");
        swapper.TurnOff();
        batteryProp?.SetGateEnabled(false);
        batteryProp?.SetDiodeEnabled(false);
    }
}
