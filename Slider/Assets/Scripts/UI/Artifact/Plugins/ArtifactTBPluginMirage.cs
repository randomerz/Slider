using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactTBPluginMirage : ArtifactTBPlugin
{
    //disable mirage button called on click based on bool flag and whnever a move is made
    //if desertMirage is true, do any mirage things. but if false, revert to normal behavior and don't enable the foggy vfx for the tile
    public int mirageIslandId;
    public ArtifactTBPluginLaser myLaserPlugin;
    [SerializeField] private List<ArtifactTBPluginLaser> laserPlugins;
    [SerializeField] private List<Sprite> mirageSprites;
    [SerializeField] private STile stile;
    private ButtonMirage buttonMirage;
    private void OnEnable()
    {
        buttonMirage = GetComponent<ButtonMirage>();
        DesertArtifact.MoveMadeOnArtifact += MoveMadeOnArtifact;
    }
    private void OnDisable()
    {
        DesertArtifact.MoveMadeOnArtifact -= MoveMadeOnArtifact;
    }
    public void MirageOnClick()
    {
        if (mirageIslandId > 0)
        {
            //Debug.Log("disablemirage");
            //DisableMirage();
            //return;
        }
        //Debug.Log("selecting");
        button.SelectButton();
    }
    private void MoveMadeOnArtifact(object sender, System.EventArgs e)
    {
        if (!MirageSTileManager.GetInstance().MirageEnabled) return; 

        DisableMirageButton();
        (int, int) cords = (button.x, button.y);
        DesertArtifact artifact = (DesertArtifact)UIArtifact.GetInstance();
        mirageIslandId = artifact.currGrid[cords];

        if (mirageIslandId < 8) 
        {
            EnableMirageButton();
        }
    }

    private void EnableMirageButton()
    {
        button.SetEmptySprite(mirageSprites[mirageIslandId - 1]);
        button.SetIslandSprite(mirageSprites[mirageIslandId - 1]);
        button.buttonAnimator.sliderImage.sprite = mirageSprites[mirageIslandId - 1]; //I honestly dunno why this line is needed
        stile.sliderCollider.isTrigger = true;
        buttonMirage.SetMirageEnabled(true);

        //Todo: copy laser TB plugin data from old tile to new tile
        ArtifactTBPluginLaser laserPlugin = laserPlugins[mirageIslandId - 1];
        myLaserPlugin.CopyDataFromMirageSource(laserPlugin);
    }

    private void DisableMirageButton()
    {
        button.RestoreDefaultEmptySprite();
        button.RestoreDefaultIslandSprite();
        button.SetSpriteToIslandOrEmpty();
        MirageSTileManager.GetInstance().DisableMirage(mirageIslandId);
        mirageIslandId = 0;
        stile.sliderCollider.isTrigger = false;
        buttonMirage.SetMirageEnabled(false);
        myLaserPlugin.ClearDataOnMirageDisable();

        DesertArtifact.MirageDisappeared?.Invoke(this, new System.EventArgs());
    }
}
