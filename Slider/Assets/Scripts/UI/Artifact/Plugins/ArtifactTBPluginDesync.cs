using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactTBPluginDesync : ArtifactTBPlugin
{
    public Sprite emptyDesynchSprite;

    private void Awake()
    {
        button = GetComponentInParent<ArtifactTileButton>();
        button.plugins.Add(this);
    }

    public override void OnPosChanged()
    {
        UpdateEmptySprite();
    }

    public void UpdateEmptySprite()
    {
        bool isDesynced = button.x == MagiTechArtifact.desynchLocation.x &&  button.y == MagiTechArtifact.desynchLocation.y;
        if(isDesynced)
            button.SetEmptySprite(emptyDesynchSprite);
        else
            button.RestoreDefaultEmptySprite();
    }
}
