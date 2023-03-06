using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CasinoDiscoBall : MonoBehaviour, ISavable
{
    public Animator ballAnimator;
    public Animator floorAnimator;
    public Collider2D ballCollider;
    private bool didFall;

    private const string SAVE_STRING = "desertDiscoBallFell";
    private const string FALL_ANIMTION = "Fall";
    private const string BROKEN_IDLE_ANIMTION = "BrokenIdle";
    
    public void Load(SaveProfile profile)
    {
        didFall = profile.GetBool(SAVE_STRING);

        if (didFall)
        {
            ballAnimator.Play(BROKEN_IDLE_ANIMTION);
            floorAnimator.Play(BROKEN_IDLE_ANIMTION);
            EnableCollider();
        }
    }

    public void Save()
    {
        SaveSystem.Current.SetBool(SAVE_STRING, didFall);
    }

    public void Fall()
    {
        if (didFall)
            return;
        didFall = true;
        SaveSystem.Current.SetBool(SAVE_STRING, didFall);

        AudioManager.Play("Glass Clink");

        ballAnimator.Play(FALL_ANIMTION);
        floorAnimator.Play(FALL_ANIMTION);
    }

    public void EnableCollider()
    {
        ballCollider.enabled = true;
    }

    public void DoScreenShake()
    {
        AudioManager.Play("Slide Explosion");
        CameraShake.Shake(1f, 0.5f); // values from Anchor
    }
}
