using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorOverrideController : MonoBehaviour
{
    public void SwapAnimator(RuntimeAnimatorController a)
    {
        this.GetComponent<Animator>().runtimeAnimatorController = a;
    }
}
