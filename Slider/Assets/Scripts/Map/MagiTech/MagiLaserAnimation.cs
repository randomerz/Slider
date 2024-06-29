using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiLaserAnimation : MonoBehaviour
{
    public MagiLaser magiLaser;
    public Animator animator;
    public Material laserBeamMaterial;
    public ParticleSystem particlesSuck;
    public ParticleSystem particlesBoom;

    private void OnDestroy() 
    {
        laserBeamMaterial.SetFloat("_TimeOffset", 0);
    }

    public void SetPowered(bool value)
    {
        animator.SetBool("Powered", value);
    }

    public void OnCharge()
    {
        particlesSuck.Play();
        
        AudioManager.Play("Laser Start", transform);
    }

    public void IncreasingCameraShake()
    {
        CameraShake.ShakeIncrease(2, 0.1f);
    }

    public void OnBurst()
    {
        CameraShake.Shake(0.75f, 0.5f);
        particlesBoom.Play();
        laserBeamMaterial.SetFloat("_TimeOffset", Time.time);
        
        magiLaser.SetEnabled(true);
    }

    public void SetEnabledFalse()
    {
        magiLaser.SetEnabled(false);
    }

    public void PowerFromLoad()
    {
        animator.Play("Loop");
        animator.SetBool("Powered", true);
    }
}
