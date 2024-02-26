using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExplodableRock : MonoBehaviour, ISavable
{
    public bool isArmed; // isArmed is not serialized!
    protected bool tryingToExplode;
    public bool isExploded;
    protected bool finishedExploding;
    public string saveString;

    public Collider2D myCollider;
    public Animator animator;
    public GameObject explosiveDecals;
    public PlayerConditionals bombSignConditional;
    public List<ParticleSystem> explosionDecalParticles = new List<ParticleSystem>();
    public List<ParticleSystem> explosionParticles = new List<ParticleSystem>();
    public List<GameObject> raycastColliderObjects = new List<GameObject>();
    public UnityEvent OnExplosionFinish;

    [Header("Collectible Fall Arc")]
    public Collectible collectible;
    [SerializeField] private Transform collectibleStart;
    [SerializeField] private Transform collectibleTarget;
    public UnityEvent OnCollectibleFall;
    [SerializeField] private float animationDuration;
    [SerializeField] private AnimationCurve xPickUpMotion;
    [SerializeField] private AnimationCurve yPickUpMotion;

    void Start()
    {
        if (saveString == null)
        {
            Debug.LogError("Rock's save string is not set! Please create a (unique) string for it to save its data to, such as 'magitechRockTile5'.");
        }
    }
    
    public virtual void Load(SaveProfile profile)
    {
        isExploded = profile.GetBool(saveString);
        if (isExploded)
        {
            FinishExploding();
            if (collectible != null)
            {
                FinishCollectibleDrop();
            }
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
            ExplodeRock();
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

    public virtual void ExplodeRock()
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
        StartCoroutine(CollectibleDrop());
        yield return new WaitForSeconds(1.5f);

        FinishExploding();
    }

    protected IEnumerator CollectibleDrop()
    {
        if (collectible == null)
            yield break;

        collectible.gameObject.SetActive(true);
        collectible.GetComponent<Collider2D>().enabled = false;
        Vector3 start = collectibleStart.transform.position;

        float t = 0;
        while (t < animationDuration)
        {
            float x = xPickUpMotion.Evaluate(t / animationDuration);
            float y = yPickUpMotion.Evaluate(t / animationDuration);
            Vector3 pos = new Vector3(Mathf.LerpUnclamped(start.x, collectibleTarget.transform.position.x, x),
                                      Mathf.LerpUnclamped(start.y, collectibleTarget.transform.position.y, y));
            
            collectible.transform.position = pos;

            yield return null;
            t += Time.deltaTime;
        }

        ParticleManager.SpawnParticle(ParticleType.SmokePoof, collectibleTarget.transform.position, collectibleTarget);

        FinishCollectibleDrop();
    }

    protected void FinishCollectibleDrop()
    {
        collectible.transform.position = collectibleTarget.transform.position;
        collectible.GetComponent<Collider2D>().enabled = true;
        collectible.getSpriteRenderer().sortingLayerName = "Entity";
        collectible.getSpriteRenderer().sortingOrder = 0;

        OnCollectibleFall?.Invoke();
    }

    public virtual void FinishExploding()
    {
        animator.SetBool("finishedExploding", true);
        finishedExploding = true;
        myCollider.enabled = false;

        foreach (GameObject go in raycastColliderObjects)
        {
            go.SetActive(false);
        }

        OnExplosionFinish?.Invoke();
    }

    // Exposed for the animation events
    public void RubbleShake()
    {
        CameraShake.Shake(0.5f, 0.75f);
    }

    public void CheckIsArmed(Condition c) => c.SetSpec(isArmed);
    public void CheckTryingToExplode(Condition c) => c.SetSpec(tryingToExplode);
    public void CheckIsExploded(Condition c) => c.SetSpec(isExploded);
    public void CheckFinishedExploding(Condition c) => c.SetSpec(finishedExploding);
}
