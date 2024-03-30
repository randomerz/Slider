using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryTimeManager : Singleton<FactoryTimeManager>
{
    [SerializeField] private PlayerPositionChanger pastPPChanger;
    [SerializeField] private PlayerPositionChanger presentPPChanger;
    //[SerializeField] private GameObject[] pastTileMaps;

    public List<GameObject> presentBobs;
    public List<GameObject> pastBobs;
    private bool sendingToPast;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start() 
    {
        if (FactoryGrid.PlayerInPast)
        {
            UpdateMapToPast();
        }
        else
        {
            SetBobTrackers(false);
        }
    }

    public static void SpawnPlayerInPast()
    {
        _instance.pastPPChanger.UPPTransform();
        
        UIEffects.FadeFromWhite();
        FactoryLightManager.SwitchLights(true);

        UpdateMapToPast();
    }

    private static void UpdateMapToPast()
    {
        PlayerInventory.ReturnAnchorFromMap();
        _instance.SetBobTrackers(true);
        (SGrid.Current as FactoryGrid).factoryMusicController.SetIsInPast(true);
    }

    public static void SpawnPlayerInPresent()
    {
        PlayerInventory.ReturnAnchorFromMap();
        _instance.SetBobTrackers(false);
        _instance.presentPPChanger.UPPTransform();
        UIEffects.FadeFromWhite();

        (SGrid.Current as FactoryGrid).factoryMusicController.SetIsInPast(false);
    }

    public static bool ShouldServerScriptBeAvailable()
    {
        return !SaveSystem.Current.GetBool("factoryDidBTTF");
    }
    
    public void SetBobTrackers(bool inPast)
    {
        // Add trackers
        foreach (GameObject go in inPast ? pastBobs : presentBobs)
        {
            go.GetComponent<TrackableItem>().SetTrackerEnabled(true);
        }

        // Remove trackers
        foreach (GameObject go in inPast ? presentBobs : pastBobs)
        {
            go.GetComponent<TrackableItem>().SetTrackerEnabled(false);
        }
    }

    public void StartSendToPastEvent()
    {
        if (sendingToPast) return;

        if (!ShouldServerScriptBeAvailable())
        {
            // Already came back from past
            SaveSystem.Current.SetBool("factoryGeneTurnedOffScript", true);
            return;
        }

        StartCoroutine(_instance.SendToPastEvent());
    }

    private IEnumerator SendToPastEvent()
    {
        sendingToPast = true;
        CameraShake.Shake(0.5f, 0.25f);
        AudioManager.PlayWithVolume("Slide Explosion", 0.25f);

        yield return new WaitForSeconds(2);

        for (int i = 0; i < 2; i++)
        {
            CameraShake.Shake(0.25f, 0.25f);
            AudioManager.PlayWithVolume("Slide Rumble", 0.25f);

            yield return new WaitForSeconds(1f);
        }

        for (int i = 0; i < 4; i++)
        {
            CameraShake.Shake(0.25f, 0.25f);
            AudioManager.PlayWithVolume("Slide Rumble", 0.25f);

            yield return new WaitForSeconds(0.5f);
        }

        for (int i = 0; i < 7; i++)
        {
            CameraShake.Shake(0.25f, 0.25f);
            AudioManager.PlayWithVolume("Slide Rumble", 0.25f);

            if (i == 1 || i == 4 || i == 6)
                FactoryLightManager.SwitchLights(true);
            if (i == 2 || i == 5)
                FactoryLightManager.SwitchLights(false);

            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.1f);

        // cut everything off
        CameraShake.StopShake();
        AudioManager.StopAllSoundAndMusic();
        AudioManager.PlayMusic("Factory");

        SpawnPlayerInPast();
        sendingToPast = false;

        SaveSystem.Current.SetBool("FactorySentToPast", true);
    }
    
    public void ShouldServerScriptBeAvailable(Condition c) => c.SetSpec(ShouldServerScriptBeAvailable());
    public void IsSendingToPast(Condition c) => c.SetSpec(_instance.sendingToPast);
}

