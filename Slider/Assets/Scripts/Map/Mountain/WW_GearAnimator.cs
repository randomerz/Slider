using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WW_GearAnimator : MonoBehaviour
{
    public enum State
    {
        Normal,
        Clicking,
        Frozen,
        NotMoving,
    }

    public Animator animator;
    public GameObject iceGameObject;
    
    [HideInInspector]
    public WW_GearAnimator nextGear;

    [HideInInspector]
    public State state;

    void Start()
    {
        
    }

    // Called by animator
    public void OnClickStart()
    {
        // If the next gear is not moving (unfrozen), try to set it to click
        if (nextGear != null && nextGear.state == State.NotMoving)
        {
            nextGear.SetCurrentState(State.NotMoving);
        }
    }

    public void SetCurrentState(State state)
    {
        this.state = state;
        switch (state)
        {
            case State.Normal:
                animator.Play("Turn");
                SetIce(false);
                break;
            case State.Clicking:
                animator.Play("Click");
                SetIce(false);
                break;
            case State.Frozen:
                animator.Play("NotMoving");
                SetIce(true);
                break;
            case State.NotMoving:
                animator.Play("NotMoving");
                SetIce(false);
                break;
        }
    }

    public void SetSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
    }

    private void SetIce(bool enabled)
    {
        iceGameObject.SetActive(enabled);

        if (enabled)
        {
            ParticleManager.SpawnParticle(ParticleType.MiniSparkle, transform.position, transform);
        }
    }
}
