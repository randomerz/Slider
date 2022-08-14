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

    public void StartCrystalPoweredSequence()
    {
        AudioManager.StopMusic("Factory");
        StartCoroutine(CrystalPoweredBuildup());
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
            if (node.nodeType == ElectricalNode.NodeType.INPUT && !FactoryGrid.IsInPast(node.gameObject))
            {
                node.StartSignal(false);
            }
        }

        foreach (var conveyor in conveyors)
        {
            conveyor.ConveyorEnabled = false;
        }

        FactoryLightManager.SwitchLights(false);
    }
}
