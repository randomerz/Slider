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
        StartCoroutine(CrystalPoweredBuildup());
    }

    private IEnumerator CrystalPoweredBuildup()
    {
        AudioManager.StopMusic("Factory");
        yield return new WaitForSeconds(2.0f);
        DoBlackout();
    }

    private void DoBlackout()
    {
        AudioManager.PlayWithVolume("Power Off", 1.0f);
        SetBlackout(true);
        FactoryLightManager.SwitchLights(false);
    }

    public void TurnEverythingBackOn()
    {
        AudioManager.PlayWithVolume("Power On", 1.0f);
        AudioManager.PlayMusic("Factory");
        SetBlackout(false);
    }

    private void SetBlackout(bool isBlackout)
    {
        Blackout = isBlackout;
        foreach (var node in allNodes)
        {
            if (node != null && node.AffectedByBlackout)
            {
                node.SetBlackout(isBlackout);
            }
        }

        foreach (var conveyor in conveyors)
        {
            conveyor.ConveyorEnabled = !isBlackout;
        }
    }
}
