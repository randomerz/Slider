using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorOverrideController : MonoBehaviour
{
    public void SwapAnimator(Animator a)
    {
        this.GetComponent<Player>().SetPlayerAnimator(a);
    }
}
