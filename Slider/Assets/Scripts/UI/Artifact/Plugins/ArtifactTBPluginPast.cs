using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactTBPluginPast : ArtifactTBPlugin
{
    [SerializeField] private Sprite pastSprite;
    [SerializeField] private Sprite pastCompletedSprite;

    public void UsePastSprite()
    {
        button.SetIslandSprite(pastSprite);
        button.SetCompletedSprite(pastCompletedSprite);
    }
}
