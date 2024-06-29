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

    [Header("GTA")]
    public PlayableDirector director;

    public List<GameObject> gameObjectsWallToEnable = new();
    public List<GameObject> gameObjectsWallToDisable = new();
    public List<GameObject> gameObjectsDoorToEnable = new();
    public List<GameObject> gameObjectsDoorToDisable = new();
    public List<Animator> animators = new();
    public DesertChadGTA desertChadGTA;
    public MagiLaser magiLaser;
    public ParticleTrail fadedLaserTrail;

    public float duckingDuration;

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
    }

    public override void ArmRock()
    {
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
    }

    public override void ExplodeRock()
    {
        if (isExploded)
            return;

        isExploded = true;
        Save();

        desertChadGTA.StartCasinoHeist();

        AudioManager.DampenMusic(this, 0.2f, duckingDuration);
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
