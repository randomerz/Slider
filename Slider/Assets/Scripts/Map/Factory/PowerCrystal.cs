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
        AudioManager.PickSound("Power On").WithPitch(1.0f).WithVolume(0.8f).AndPlay();
        (SGrid.Current as FactoryGrid).factoryMusicController.DoFactoryStinger();
        
        yield return new WaitForSeconds(1.5f);

        AudioManager.PickSound("Power On").WithPitch(1.2f).WithVolume(0.9f).AndPlay();
        
        yield return new WaitForSeconds(1.5f);

        AudioManager.PickSound("Power On").WithPitch(1.4f).WithVolume(1.0f).AndPlay();

        yield return new WaitForSeconds(FactoryMusicController.FACTORY_STINGER_DURATION - 4f);
        
        CameraShake.Shake(0.25f, 0.25f);
        AudioManager.PlayWithVolume("Slide Explosion", 0.2f);

        yield return new WaitForSeconds(0.5f);
        
        CameraShake.Shake(0.35f, 0.3f);
        AudioManager.PlayWithVolume("Slide Explosion", 0.4f);

        yield return new WaitForSeconds(0.5f);
        
        CameraShake.Shake(1f, 0.35f);
        AudioManager.Play("Slide Explosion");
        AudioManager.Play("Power Off");
        DoBlackout();

        yield return new WaitForSeconds(2);

        AudioManager.StopMusic("Factory");
    }

    private void DoBlackout()
    {
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
