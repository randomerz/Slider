using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodableRock : MonoBehaviour, ISavable
{
    public bool isArmed; // isArmed is not serialized!
    public bool isExploded;
    protected bool tryingToExplode;
    public string saveString;

    public Collider2D myCollider;
    public Animator animator;
    public GameObject explosiveDecals;
    public PlayerConditionals bombSignConditional;
    public List<ParticleSystem> explosionDecalParticles = new List<ParticleSystem>();
    public List<ParticleSystem> explosionParticles = new List<ParticleSystem>();
    public List<GameObject> raycastColliderObjects = new List<GameObject>();

    void Start()
    {
        if (saveString == null)
        {
            Debug.LogError("Rock's save string is not set! Please create a (unique) string for it to save its data to, such as 'magitechRockTile5'.");
        }
    }
    
    public void Load(SaveProfile profile)
    {
        isExploded = profile.GetBool(saveString, false);
        if (isExploded)
        {
            FinishExploding();
        }

        if (isArmed && !isExploded)
        {
            explosiveDecals.SetActive(true);
            bombSignConditional.DisableConditionals();
        }
    }

    public void Save()
    {
        SaveSystem.Current.SetBool(saveString, isExploded);
    }


    public virtual void ArmRock()
    {
        if (isArmed || isExploded)
            return;
        isArmed = true;

        AudioManager.Play("Hat Click");

        explosiveDecals.SetActive(true);
        bombSignConditional.DisableConditionals();

        foreach (ParticleSystem ps in explosionDecalParticles)
        {
            ps.Play();
        }

        if (tryingToExplode)
        {
            Explode();
        }
    }

    public virtual void SetTryExplodeRock(bool value)
    {
        if (isArmed && value)
        {
            ExplodeRock();
            return;
        }

        tryingToExplode = value;
    }

    public void ExplodeRock()
    {
        if (isExploded)
            return;

        isExploded = true;
        Save();

        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        explosiveDecals.SetActive(false);
        animator.SetBool("explode", true);
        AudioManager.Play("Slide Explosion");

        CameraShake.Shake(0.75f, 1);
        foreach (ParticleSystem p in explosionParticles)
        {
            p.Play();
        }

        yield return new WaitForSeconds(1.5f);

        FinishExploding();
    }

    public virtual void FinishExploding()
    {
        animator.SetBool("finishedExploding", true);
        myCollider.enabled = false;

        foreach (GameObject go in raycastColliderObjects)
        {
            go.SetActive(false);
        }
    }

    // Exposed for the animation events
    public void RubbleShake()
    {
        CameraShake.Shake(0.5f, 0.75f);
    }

    public void CheckIsArmed(Condition c) => c.SetSpec(isArmed);
    public void CheckIsExploded(Condition c) => c.SetSpec(isExploded);
    public void CheckTryingToExplode(Condition c) => c.SetSpec(tryingToExplode);
}
