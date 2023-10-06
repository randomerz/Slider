using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantExplodableRock : ExplodableRock, ISavable
{
    public List<GameObject> laserRaycastColliders;

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
        explosiveDecals.SetActive(false);
        animator.SetBool("explode", true);
        // AudioManager.Play("Slide Explosion");

        CameraShake.Shake(0.75f, 1);
        foreach (ParticleSystem p in explosionParticles)
        {
            p.Play();
        }

        yield return new WaitForSeconds(1.5f);

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
