using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSetFrame : MonoBehaviour
{
    public Animator[] animators;
    public string[] animationNames;
    [Tooltip("This is from [0, 1]")]
    public float animationTime;
    public bool setOnAwake = true;
    public bool setOnStart = false;

    [Header("For Random Start")]
    public bool doRandom;
    public float animationDuration;

    private void Awake() 
    {
        if (setOnAwake) UpdateAnimation();
    }

    private void Start() 
    {
        if (setOnStart) UpdateAnimation();
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
