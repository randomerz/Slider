﻿using UnityEngine;


class CaveArtifactTileButton : ArtifactTileButton
{
    public bool isLit = true;

    [SerializeField]
    private Sprite islandDarkSprite;
    [SerializeField]
    private Sprite islandLitSprite;

    private new void Start()
    {
        base.Start();

        CheckLit();
    }

    private void OnEnable()
    {
        CheckLit();
        SGrid.OnSTileEnabled += STileEnabled;
        UIArtifact.OnButtonInteract += ButtonInteract;
        CaveLight.OnLightSwitched += LightSwitched;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        SGrid.OnSTileEnabled -= STileEnabled;
        UIArtifact.OnButtonInteract -= ButtonInteract;
        CaveLight.OnLightSwitched -= LightSwitched;
    }

    private void STileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        CheckLit();
    }

    private void ButtonInteract(object sender, System.EventArgs e)
    {
        CheckLit();
    }

    private void LightSwitched(object sender, CaveLight.OnLightSwitchedArgs e)
    {
        CheckLit();
    }

    public void CheckLit()
    {
        if (MyStile != null && TileIsActive)
        {
            isLit = (MyStile as CaveSTile).GetTileLit(this.x, this.y);
            islandSprite = isLit ? islandLitSprite : islandDarkSprite;
            SetSpriteToIslandOrEmpty();
        }
    }
}