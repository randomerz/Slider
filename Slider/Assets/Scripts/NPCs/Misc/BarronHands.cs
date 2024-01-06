using System.Collections.Generic;
using UnityEngine;

public class BarronHands : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer barronSpriteRenderer;
    private readonly List<string> animationNames = new();
    private const string ANIMATION_NAME_PREFIX = "BarronHands_";
    
    private void Awake()
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips) 
        {
            string n = clip.name.Substring(ANIMATION_NAME_PREFIX.Length);
            animationNames.Add(n);
        }
    }

    private void LateUpdate()
    {
        spriteRenderer.flipX = barronSpriteRenderer.flipX;
    }

    /// <summary>
    /// List of supported animations (probably):
    /// - Raised
    /// - Walking
    /// - Down
    /// - Close
    /// - Pointing
    /// - Waving
    /// - Shrug
    /// </summary>
    /// <param name="animationName"></param>
    public void SetAnimation(string animationName)
    {
        if (!animationNames.Contains(animationName))
        {
            Debug.LogWarning($"Couldn't find animation with name '{animationName}'");
            return;
        }

        animator.Play(animationName);
    }

    public void SetIsWalking(bool value)
    {
        animator.SetBool("isWalking", value);
    }
}