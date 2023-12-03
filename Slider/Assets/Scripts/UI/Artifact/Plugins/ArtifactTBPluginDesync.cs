using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ArtifactTBPluginDesync : ArtifactTBPlugin
{
    public Sprite emptyDesyncSprite;

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
        bool isDesynced = button.x == MagiTechArtifact.desyncLocation.x &&  button.y == MagiTechArtifact.desyncLocation.y;
        if(isDesynced)
            button.SetEmptySprite(emptyDesyncSprite);
        else
            button.RestoreDefaultEmptySprite();
    }
}
