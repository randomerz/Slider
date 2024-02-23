using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactTBPluginMirage : ArtifactTBPlugin
{
    //disable mirage button called on click based on bool flag and whnever a move is made
    //if desertMirage is true, do any mirage things. but if false, revert to normal behavior and don't enable the foggy vfx for the tile
    public int mirageIslandId;
    public int buttonIslandId;
    public ArtifactTBPluginLaser myLaserPlugin;
    [SerializeField] private List<ArtifactTBPluginLaser> laserPlugins;
    [SerializeField] private List<Sprite> mirageSprites;
    [SerializeField] private STile stile;
    [SerializeField] private ButtonMirage buttonMirage;

    private void OnEnable()
    {
        DesertArtifact.MoveMadeOnArtifact += MoveMadeOnArtifact;
    }
    private void OnDisable()
    {
        DesertArtifact.MoveMadeOnArtifact -= MoveMadeOnArtifact;
    }

    public void MirageOnClick()
    {
        button.SelectButton();
    }

    private void MoveMadeOnArtifact(object sender, System.EventArgs e)
    {
        CheckForMirage();
    }

    private void CheckForMirage()
    {
        if (!MirageSTileManager.GetInstance().MirageEnabled) return; 

        DisableMirageButton();
        (int, int) cords = (button.x, button.y);
        DesertArtifact artifact = (DesertArtifact)UIArtifact.GetInstance();
        mirageIslandId = artifact.currGrid[cords];

        if (mirageIslandId < 8) 
        {
            EnableMirageButton(mirageIslandId);
        }
    }

    public void EnableMirageButton(int id)
    {
        mirageIslandId = id;
        button.SetEmptySprite(mirageSprites[mirageIslandId - 1]);
        button.SetIslandSprite(mirageSprites[mirageIslandId - 1]);
        button.SetSpriteToIsland();
        stile.sliderCollider.isTrigger = true;
        buttonMirage.SetMirageEnabled(true);

        ArtifactTBPluginLaser laserPlugin = laserPlugins[mirageIslandId - 1];
        myLaserPlugin.laserUIData.CopyDataFromMirageSource(laserPlugin.laserUIData);
    }

    public void DisableMirageButton()
    {
        button.RestoreDefaultEmptySprite();
        button.RestoreDefaultIslandSprite();
        button.SetSpriteToIslandOrEmpty();
        MirageSTileManager.GetInstance().DisableMirageTile(mirageIslandId);
        mirageIslandId = 0;
        stile.sliderCollider.isTrigger = false;
        buttonMirage.SetMirageEnabled(false);
        myLaserPlugin.laserUIData?.ClearDataOnMirageDisable();

        DesertArtifact.MirageDisappeared?.Invoke(this, new System.EventArgs());
    }
}
