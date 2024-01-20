using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{
    public PlayerConditionals playerConditionals;
    public SpriteRenderer spriteRenderer;
    public Sprite emptyBottleSprite;


    private void Start()
    {
        if (!SaveSystem.Current.GetBool(MirageSTileManager.MIRAGE_ENABLED_SAVE_STRING))
        {
            gameObject.SetActive(false);
        }
        else
        {
            FinishDrinkBottle();
        }
    }

    public void PlayerDrinkBottle()
    {
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position, transform);
        AudioManager.Play("MagicChimes2");
        MirageSTileManager.GetInstance().EnableMirage();
        FinishDrinkBottle();
    }

    private void FinishDrinkBottle()
    {
        spriteRenderer.sprite = emptyBottleSprite;
        playerConditionals.DisableConditionals();
    }
}
