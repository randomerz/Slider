using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactTBPluginPast : ArtifactTBPlugin
{
    [SerializeField] Sprite pastSprite;

    public void UsePastSprite()
    {
        button.SetIslandSprite(pastSprite);
    }
}
