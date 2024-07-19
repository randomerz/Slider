using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class DesertGTA : ExplodableRock
{
    private const string BLEW_UP_CASINO_WALL_SAVE_STRING = "DesertBlewUpCasinoWall";
    private const string MAGITECH_LASER_SAVE_STRING = "MagitechLaserEnabled";
    private const string DESERT_LASER_SAVE_STRING = "MagitechDesertLaser";
    private const string MAGITECH_LEVER_SAVE_STRING = "magiTechLaserLever";
    private const string MAGITECH_CHAD_ROCK_STRING = "magitechPortalChadRock";

    [Header("GTA")]
    public PlayableDirector director;

    public List<GameObject> gameObjectsWallToEnable = new();
    public List<GameObject> gameObjectsWallToDisable = new();
    public List<GameObject> gameObjectsDoorToEnable = new();
    public List<GameObject> gameObjectsDoorToDisable = new();
    public List<Animator> animators = new();
    public DesertChadGTA desertChadGTA;
    public Portal portal;
    public MagiLaser magiLaser;
    public ParticleTrail fadedLaserTrail;

    private const float DUCKING_DURATION = 15;

    private void Awake()
    {
        // foreach (GameObject go in raycastColliderObjects)
        // {
        //     go.SetActive(false);
        // }
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);

        if (isExploded)
        {
            UpdateExplosionWallGameObjects();
            UpdateExplosionDoorGameObjects();
            FinishAnimators();
        }
        else if (profile.GetBool(DesertChadGTA.CHAD_STARTED_HEIST_SAVE_STRING))
        {
            ArmRock();
        }
    }

    public override void ArmRock()
    {
        if (isArmed || isExploded)
            return;

        if (!PlayerInventory.Contains("Explosives", Area.Military))
        {
            AudioManager.Play("Artifact Error");
            return;
        }

        base.ArmRock();

        foreach (GameObject go in raycastColliderObjects)
        {
            go.SetActive(true);
        }

        if (SaveSystem.Current.GetBool(MAGITECH_CHAD_ROCK_STRING))
        {
            desertChadGTA.StartCasinoHeist();
            portal.SetPlayerAllowedToUse(false);
        }
    }

    public void ChadWentThroughPortal()
    {
        if (isExploded)
        {
            Debug.LogError($"Called ChadWentThroughPortal even though it was already exploded. This shouldn't happen! Skipping explosion.");
            return;
        }

        StartCoroutine(DoAudioBuildUp());
        desertChadGTA.chadNPC.gameObject.SetActive(false);
    }

    private IEnumerator DoAudioBuildUp()
    {
        AudioManager.DampenMusic(this, 0.2f, DUCKING_DURATION);
        AudioManager.Play("Portal");

        yield return new WaitForSeconds(3);

        AudioManager.Play("Laser Start");

        yield return new WaitForSeconds(2.75f);

        CameraShake.ShakeIncrease(2, 0.1f);

        yield return new WaitForSeconds(4.5f - 2.75f);
        
        fadedLaserTrail.SpawnParticleTrail(shouldRepeat: false);
        AudioManager.PlayWithVolume("Hat Click", 0.5f);
        yield return new WaitForSeconds(0.1f);
        AudioManager.PlayWithVolume("Hat Click", 0.5f);
        yield return new WaitForSeconds(0.1f);
        AudioManager.PlayWithVolume("Hat Click", 0.5f);

        yield return new WaitForSeconds(5.5f - 4.5f - 0.2f);

        magiLaser.EnableLaser();

        CameraShake.Shake(0.75f, 0.5f);
        
        yield return new WaitForSeconds(1);

        ExplodeRock();
    }

    public override void ExplodeRock()
    {
        if (isExploded)
            return;

        isExploded = true;
        Save();

        portal.SetPlayerAllowedToUse(true);
        director.Play();
    }

    public override void FinishExploding()
    {
        finishedExploding = true;
        SaveSystem.Current.SetBool(BLEW_UP_CASINO_WALL_SAVE_STRING, true);

        foreach (GameObject go in raycastColliderObjects)
        {
            go.SetActive(false);
        }
    }

    // Exposed for director
    public void DisableMagitechLaser()
    {
        FinishExploding();
        
        SaveSystem.Current.SetBool(MAGITECH_LASER_SAVE_STRING, false);
        SaveSystem.Current.SetBool(DESERT_LASER_SAVE_STRING, false);
        SaveSystem.Current.SetBool(MAGITECH_LEVER_SAVE_STRING, false);

        magiLaser.DisableLaser();
        fadedLaserTrail.SpawnParticleTrail(shouldRepeat: false);
    }

    public void UpdateExplosionWallGameObjects()
    {
        foreach (GameObject go in gameObjectsWallToEnable)
        {
            go.SetActive(true);
        }
        foreach (GameObject go in gameObjectsWallToDisable)
        {
            go.SetActive(false);
        }
    }

    public void UpdateExplosionDoorGameObjects()
    {
        foreach (GameObject go in gameObjectsDoorToEnable)
        {
            go.SetActive(true);
        }
        foreach (GameObject go in gameObjectsDoorToDisable)
        {
            go.SetActive(false);
        }
    }

    public void FinishAnimators()
    {
        foreach (Animator a in animators)
        {
            a.SetBool("finishedExploding", true);
        }
    }
}
