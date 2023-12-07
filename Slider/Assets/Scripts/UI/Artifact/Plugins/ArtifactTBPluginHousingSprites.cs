using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactTBPluginHousingSprites : ArtifactTBPlugin
{
    [SerializeField] private Sprite housingSprite;
    [SerializeField] private Sprite housingCompletedSprite;


    public void UseHousingSprite()
    {
        button.SetIslandSprite(housingSprite);
        button.SetCompletedSprite(housingCompletedSprite);

        
    }
}
