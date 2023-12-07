using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantExplodableRock : ExplodableRock, ISavable
{
    public List<GameObject> laserRaycastColliders;

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);

        if (isExploded && collectible != null)
        {
            FinishCollectibleDrop();
        }
    }

    public override void ArmRock()
    {
        if (isArmed || isExploded)
            return;
        isArmed = true;

        AudioManager.Play("Hat Click");

        explosiveDecals.SetActive(true);
        // bombSignConditional.DisableConditionals();

        foreach (ParticleSystem ps in explosionDecalParticles)
        {
            ps.Play();
        }
    }

    public override void SetTryExplodeRock(bool value)
    {
        tryingToExplode = value;
    }

    /// <summary>
    /// 0 - Fade to nothing. 1 - Cover in smoke. 2 - Instantly disappaer.
    /// </summary>
    /// <param name="variation"></param>
    public void ExplodeRock(int variation=0)
    {
        if (isExploded)
            return;

        isExploded = true;
        Save();

        StartCoroutine(Explode(variation));
    }

    private IEnumerator Explode(int variation=0)
    {
        switch (variation)
        {
            case 2:
                CameraShake.ShakeIncrease(1f, 0.25f);

                yield return new WaitForSeconds(1.25f);
                
                AudioManager.Play("Pop");
                
                StartCoroutine(CollectibleDrop());
                FinishExploding();

                yield break;
            case 0:
            case 1:
            default:
                CameraShake.ShakeIncrease(2f, 0.25f);

                yield return new WaitForSeconds(3f);
                break;
        }

        explosiveDecals.SetActive(false);
        animator.SetBool("explode", true);

        if (variation == 0)
        {
            AudioManager.Play("Bwomp");
            StartCoroutine(CollectibleDrop());

            yield return new WaitForSeconds(1f);
        }
        else if (variation == 1)
        {
            AudioManager.Play("Rumble Decrease 5s");

            CameraShake.Shake(0.75f, 1);
            foreach (ParticleSystem p in explosionParticles)
            {
                p.Play();
            }

            yield return new WaitForSeconds(2f);

            StartCoroutine(CollectibleDrop());
        }

        FinishExploding();
    }

    public override void FinishExploding()
    {
        base.FinishExploding();

        foreach (GameObject go in laserRaycastColliders)
        {
            go.SetActive(false);
        }
    }
    

    
}
