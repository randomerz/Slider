using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiTechInitialPortalTease : MonoBehaviour
{
    private float timeTillAnimation;
    public float animationCooldown = 10;
    public float playerDeactivateDistance = 5;
    public Animator animator;

    public ParticleSystem constantParticles;
    public ParticleSystem burstParticles;

    public GameObject realPortal;

    public MagiTechArtifact magiTechArtifact;
    public GameObject uiPortalTracker;

    public SpriteRenderer teaseSpriteRenderer;
    private bool initialPortalOpened = false;
    
    void Start()
    {
        if (SaveSystem.Current.GetBool("magitechInitialPortalOpened"))
        {
            EnableRealPortal(true);
            return;
        }

        ResetTimer();
    }

    void Update()
    {
        if (Vector3.Distance(Player.GetPosition(), transform.position) < playerDeactivateDistance)
        {
            if (constantParticles.isPlaying)
            {
                constantParticles.Stop();
            }
            ResetTimer();
            return;
        }
        else 
        {
            if (!constantParticles.isPlaying)
            {
                constantParticles.Play();
            }
        }

        timeTillAnimation -= Time.deltaTime;
        if (timeTillAnimation <= 0)
        {
            animator.SetTrigger("zap");
            ResetTimer();
        }

        //UpdateUITracker();
    }

    public void ResetTimer()
    {
        timeTillAnimation = animationCooldown + Random.Range(-1, 1);
    }

    public void DoBurst()
    {
        burstParticles.Play();
    }

    public void StartOpeningPortal()
    {
        StartCoroutine(OpenPortal());
    }

    private IEnumerator OpenPortal()
    {
        yield return new WaitForSeconds(0.5f);
        AudioManager.Play("MagicChimes1");
        CameraShake.ShakeIncrease(2, 2);

        yield return new WaitForSeconds(2);
        EnableRealPortal(false);
    }

    public void EnableRealPortal(bool fromSave=false)
    {
        SaveSystem.Current.SetBool("magitechInitialPortalOpened", true);
        initialPortalOpened = true;
        if (!fromSave)
        {
            FlashScreen();

            AudioManager.Play("MagicChimes2");
            AudioManager.Play("Rumble Decrease 2.5s");
            AudioManager.Play("Portal Open");

            ParticleManager.SpawnParticle(
                ParticleType.SmokePoof, realPortal.transform.position, realPortal.transform
            );

        }

        teaseSpriteRenderer.enabled = false;
        animator.enabled = false;
        realPortal.SetActive(true);

        // // to prevent player from going in during chad cutscene
        // realPortal.GetComponent<Portal>().SetPlayerAllowedToUse(false);
    }

    private void FlashScreen()
    {
        // This should last 0.5 sec
        UIEffects.FlashWhite(speed: 2);
    }
    /*
    private void UpdateUITracker()
    {
        if (magiTechArtifact == null || uiPortalTracker == null)
            return;

        bool displayingPast = magiTechArtifact.IsDisplayingPast();
        uiPortalTracker.SetActive(displayingPast && realPortal.activeInHierarchy);
    }
    */
    public void IsRealPortalEnabled(Condition c)
    {
        c.SetSpec(initialPortalOpened);
    }
}
