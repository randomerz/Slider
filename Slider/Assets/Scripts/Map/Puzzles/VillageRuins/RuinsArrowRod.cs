using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuinsArrowRod : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite offSprite;
    public Sprite onSprite;
    public ParticleSystem rodParticles;

    public SpriteRenderer runesSpriteRenderer;
    public FlashWhiteSprite runesFlash;

    private bool onLastFrame;

    private void Awake()
    {
        SetRod(false);
    }

    private void LateUpdate()
    {
        onLastFrame = runesSpriteRenderer.enabled;
    }

    public void SetRod(bool value)
    {
        if (value)
        {
            spriteRenderer.sprite = onSprite;
            rodParticles.Play();

            if (!onLastFrame) // if was off last frame
                runesFlash.Flash(1);
            runesSpriteRenderer.enabled = value;
        }
        else
        {
            spriteRenderer.sprite = offSprite;
            rodParticles.Stop();

            runesSpriteRenderer.enabled = value;
        }
    }
}
