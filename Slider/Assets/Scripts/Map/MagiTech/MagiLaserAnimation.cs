using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiLaserAnimation : MonoBehaviour
{
    public MagiLaser magiLaser;
    public Animator animator;

    public void SetPowered(bool value)
    {
        animator.SetBool("Powered", value);
    }

    public void IncreasingCameraShake()
    {
        CameraShake.ShakeIncrease(2, 0.1f);
    }

    public void OnBurst()
    {
        CameraShake.Shake(0.75f, 0.5f);
        magiLaser.SetEnabled(true);
    }

    public void SetEnabledFalse()
    {
        magiLaser.SetEnabled(false);
    }
}
