using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCrystal : Singleton<PowerCrystal>, ISavable
{
    private ElectricalNode[] _allNodes;

    private bool _didInit = false;

    private bool _blackout = false;
    public static bool Blackout => _instance != null && _instance._blackout;

    private void Awake()
    {
        if (!_didInit)
        {
            Init();
        }
    }

    private void Init()
    {
        InitializeSingleton();
        _didInit = true;
        _allNodes = GameObject.FindObjectsOfType<ElectricalNode>();
    }

    private void Start()
    {
        SetBlackout(_blackout);
    }

    public void CheckBlackout(Condition cond)
    {
        cond.SetSpec(_blackout);
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
        FactoryLightManager.SwitchLights(true);
    }

    private void SetBlackout(bool isBlackout)
    {
        _blackout = isBlackout;
    }

    public void Save()
    {
        SaveSystem.Current.SetBool("Blackout", _blackout);
    }

    public void Load(SaveProfile profile)
    {
        _blackout = profile.GetBool("Blackout");
    }
}
