using System;
using System.Collections;

using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FactoryTimeManager : Singleton<FactoryTimeManager>
{
    [SerializeField] private PlayerPositionChanger pastPPChanger;
    [SerializeField] private PlayerPositionChanger presentPPChanger;
    //[SerializeField] private GameObject[] pastTileMaps;

    private void Awake()
    {
        InitializeSingleton();
    }

    public static void SpawnPlayerInPast()
    {
        EnableAnchorsInPast();

        //foreach (GameObject go in _instance.pastTileMaps)
        //{
        //    go.SetActive(true);
        //}

        _instance.pastPPChanger.UPPTransform();

        UIEffects.FadeFromWhite();
        FactoryLightManager.SwitchLights(true);

    }

    public static void SpawnPlayerInPresent()
    {
        EnableAnchorsInPresent();
        _instance.presentPPChanger.UPPTransform();
        UIEffects.FadeFromWhite();
    }

    public static void EnableAnchorsInPast()
    {
        EnableAnchorsInTime(true);
    }

    public static void EnableAnchorsInPresent()
    {
        EnableAnchorsInTime(false);
    }

    public void StartSendToPastEvent()
    {
        StartCoroutine(_instance.SendToPastEvent());
    }

    private IEnumerator SendToPastEvent()
    {
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

        SpawnPlayerInPast();
    }

    public static void EnableAnchorsInTime(bool inPast)
    {
        Anchor[] anchors = GameObject.FindObjectsOfType<Anchor>(true);
        foreach (var anchor in anchors)
        {
            bool playerHoldingAnchor = Player.GetPlayerAction().HasItem("Anchor");
            if (!playerHoldingAnchor)
            {
                bool anchorInWrongTime = inPast != FactoryGrid.IsInPast(anchor.gameObject);
                if (anchorInWrongTime)
                {
                    anchor.UnanchorTile();
                    anchor.gameObject.SetActive(false);
                }
                else
                {
                    anchor.gameObject.SetActive(true);
                }
            }
        }
    }
}

