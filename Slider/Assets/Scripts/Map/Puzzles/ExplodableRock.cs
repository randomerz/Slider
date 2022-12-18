using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodableRock : MonoBehaviour, ISavable
{

    public bool isExploded;
    public string saveString;

    public Collider2D myCollider;
    public Animator animator;
    public List<ParticleSystem> explosionParticles = new List<ParticleSystem>();

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
    }

    public void Save()
    {
        SaveSystem.Current.SetBool(saveString, isExploded);
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
        animator.SetBool("explode", true);

        foreach (ParticleSystem p in explosionParticles)
        {
            p.Play();
        }

        yield return null;

        FinishExploding();
    }

    public void FinishExploding()
    {
        animator.SetBool("finishedExploding", true);
        myCollider.enabled = false;
    }
}
