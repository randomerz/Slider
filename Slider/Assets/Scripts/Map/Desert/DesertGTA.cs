using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class DesertGTA : ExplodableRock
{
    [Header("GTA")]
    public PlayableDirector director;

    public List<GameObject> gameObjectsWallToEnable = new();
    public List<GameObject> gameObjectsWallToDisable = new();
    public List<GameObject> gameObjectsDoorToEnable = new();
    public List<GameObject> gameObjectsDoorToDisable = new();
    public List<Animator> animators = new();

    public float duckingDuration;

    private void Awake()
    {
        foreach (GameObject go in raycastColliderObjects)
        {
            go.SetActive(false);
        }
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

        AudioManager.DampenMusic(this, 0.2f, duckingDuration);
        director.Play();
    }

    public override void FinishExploding()
    {
        finishedExploding = true;

        foreach (GameObject go in raycastColliderObjects)
        {
            go.SetActive(false);
        }
    }

    // Exposed for director
    public void DisableMagitechLaser()
    {
        Debug.Log("TODO: Disable laser!");
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
