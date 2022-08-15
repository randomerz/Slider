using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCrystal : MonoBehaviour
{
    private ElectricalNode[] allNodes;
    private Conveyor[] conveyors;

    public static bool Blackout { get; private set; }

    private void Start()
    {
        Blackout = false;
        allNodes = GameObject.FindObjectsOfType<ElectricalNode>();
        conveyors = GameObject.FindObjectsOfType<Conveyor>();
    }

    public void CheckBlackout(Condition cond)
    {
        cond.SetSpec(Blackout);
    }

    public void StartCrystalPoweredSequence()
    {
        AudioManager.StopMusic("Factory");
        StartCoroutine(CrystalPoweredBuildup());
    }

    public void TurnEverythingBackOn()
    {
        AudioManager.PlayWithVolume("Power On", 1.0f);
        AudioManager.PlayMusic("Factory");
        Blackout = false;
        foreach (var node in allNodes)
        {
            if (node != null && !FactoryGrid.IsInPast(node.gameObject))
            {
                node.OnPowered?.Invoke(new ElectricalNode.OnPoweredArgs { powered = node.Powered });
            }
        }

        foreach (var conveyor in conveyors)
        {
            conveyor.ConveyorEnabled = true;
        }
    }

    private IEnumerator CrystalPoweredBuildup()
    {
        yield return new WaitForSeconds(2.0f);
        DoBlackout();
    }

    private void DoBlackout()
    {
        Blackout = true;
        AudioManager.PlayWithVolume("Power Off", 1.0f);
        foreach (var node in allNodes)
        {
            if (!FactoryGrid.IsInPast(node.gameObject))
            {
                node.OnPowered?.Invoke(new ElectricalNode.OnPoweredArgs { powered = node.Powered });
            }
        }

        foreach (var conveyor in conveyors)
        {
            conveyor.ConveyorEnabled = false;
        }

        FactoryLightManager.SwitchLights(false);
    }
}
