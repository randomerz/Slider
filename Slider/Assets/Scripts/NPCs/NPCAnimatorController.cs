using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAnimatorController : MonoBehaviour
{
    public Animator myAnimator;
    
    public void SetBoolToTrue(string str)
    {
        myAnimator.SetBool(str, true);
    }
    public void SetBoolToFalse(string str)
    {
        myAnimator.SetBool(str, false);
    }
}
