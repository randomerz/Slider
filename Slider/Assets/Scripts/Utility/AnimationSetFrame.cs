using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSetFrame : MonoBehaviour
{
    public Animator[] animators;
    public string[] animationNames;
    public float animationTime;
    public bool setOnAwake = true;

    [Header("For Random Start")]
    public bool doRandom;
    public float animationDuration;

    private void Awake() 
    {
        if (setOnAwake) UpdateAnimation();
    }

    public void UpdateAnimation() 
    {
        if (doRandom) 
            animationTime = Random.Range(0, animationDuration);

        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].Play(animationNames[i], 0, animationTime);
        }
    }
}
