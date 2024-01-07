using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCrystal : Singleton<PowerCrystal>, ISavable
{
    private bool _blackout = false;
    private bool _wentToPast = false;
    public static bool Blackout => _instance != null && _instance._blackout;

    public delegate void HandleBlackout();
    public static event HandleBlackout blackoutStarted;
    public static event HandleBlackout blackoutEnded;

    private void Awake()
    {
        InitializeSingleton();
    }

    public void Save()
    {
        SaveSystem.Current.SetBool("FactoryBlackout", _blackout);
    }

    public void Load(SaveProfile profile)
    {
        _blackout = profile.GetBool("FactoryBlackout");
        _wentToPast = profile.GetBool("FactorySentToPast");

        SetBlackout(_blackout && !_wentToPast);
    }

    public void CheckBlackout(Condition cond)
    {
        cond.SetSpec(_blackout);
    }

    public void StartCrystalPoweredSequence()
    {
        if (!_blackout && !_wentToPast)
        {
            StartCoroutine(CrystalPoweredBuildup());
        }
    }

    private IEnumerator CrystalPoweredBuildup()
    {
        AudioManager.Play("Slide Explosion");
        (SGrid.Current as FactoryGrid).factoryMusicController.DoFactoryStinger();

        yield return new WaitForSeconds(2);
        
        DoBlackout();

        yield return new WaitForSeconds(FactoryMusicController.FACTORY_STINGER_DURATION - 2);

        AudioManager.StopMusic("Factory");
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

        if (isBlackout)
        {
            blackoutStarted?.Invoke();
        }
        else
        {
            blackoutEnded?.Invoke();
        }
    }
}
