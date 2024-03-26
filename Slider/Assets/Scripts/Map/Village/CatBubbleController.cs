using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatBubbleController : MonoBehaviour
{
    public Animator bubbleAnimator;
    public SpriteRenderer bubbleSpriteRenderer;
    public SpriteRenderer electricitySpriteRenderer;

    public void SetBubbleActive(bool value)
    {
        ParticleManager.SpawnParticle(ParticleType.MiniSparkle, bubbleSpriteRenderer.transform.position, bubbleSpriteRenderer.transform);
        ParticleManager.SpawnParticle(ParticleType.MiniSparkle, bubbleSpriteRenderer.transform.position, bubbleSpriteRenderer.transform);
        ParticleManager.SpawnParticle(ParticleType.MiniSparkle, bubbleSpriteRenderer.transform.position, bubbleSpriteRenderer.transform);
        ParticleManager.SpawnParticle(ParticleType.MiniSparkle, bubbleSpriteRenderer.transform.position, bubbleSpriteRenderer.transform);

        bubbleSpriteRenderer.enabled = value;
        electricitySpriteRenderer.enabled = value;

        if (value)
        {
            bubbleAnimator.SetTrigger("formBubble");
        }
    }
}
