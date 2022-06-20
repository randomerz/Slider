using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuinsArrowRod : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite offSprite;
    public Sprite onSprite;
    public ParticleSystem rodParticles;

    public void SetRod(bool value)
    {
        if (value)
        {
            spriteRenderer.sprite = onSprite;
            rodParticles.Play();
        }
        else
        {
            spriteRenderer.sprite = offSprite;
            rodParticles.Stop();
        }
    }
}
