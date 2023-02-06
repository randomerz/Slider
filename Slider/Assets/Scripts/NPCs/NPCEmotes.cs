using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCEmotes : MonoBehaviour
{
    public enum Emotes
    {
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

    void Start()
    {
        xOffset = Mathf.Abs(transform.localPosition.x);
    }

    public void PlayEmote(Emotes emote)
    {

    }

    public void StopEmote() 
    {

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
