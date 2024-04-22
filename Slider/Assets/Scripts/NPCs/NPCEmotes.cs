using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCEmotes : MonoBehaviour
{
    public enum Emotes
    {
        None,
        Sparkle,
        Exclamation,
        Question,
        Annoyed,
        Sweat,
    }

    [HideInInspector] public bool npcDefaultFacesRight;
    [HideInInspector] public float xOffset;
    public SpriteRenderer npcSpriteRenderer;
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    private Emotes currentEmote;

    void Awake()
    {
        xOffset = Mathf.Abs(transform.localPosition.x);
    }

    public void SetEmote(Emotes emote)
    {
        UpdateEmotePosition();

        if (currentEmote == emote)
            return;
        currentEmote = emote;

        if (emote == Emotes.None)
        {
            SetEmoteActive(false);
            return;
        }

        SetEmoteActive(true);
        animator.Play(emote.ToString());
    }

    public void SetEmoteActive(bool value) 
    {
        spriteRenderer.enabled = value;
    }

    public void UpdateEmotePosition()
    {
        if (!npcDefaultFacesRight) // default faces left
        {
            if (!npcSpriteRenderer.flipX) // not flipped
            {
                SetEmotePosition(true);
            }
            else
            {
                SetEmotePosition(false);
            }
        }
        else // default faces right
        {
            if (!npcSpriteRenderer.flipX) // not flipped
            {
                SetEmotePosition(false);
            }
            else
            {
                SetEmotePosition(true);
            }
        }
    }

    private void SetEmotePosition(bool isOnLeft)
    {
        transform.localPosition = new Vector3(xOffset * (isOnLeft ? -1 : 1), transform.localPosition.y);
    }
}
