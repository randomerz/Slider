using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactTBPluginMirage : ArtifactTBPlugin
{
    //disable mirage button called on click based on bool flag and whnever a move is made
    //if desertMirage is true, do any mirage things. but if false, revert to normal behavior and don't enable the foggy vfx for the tile
    public int mirageIslandId;
    [SerializeField] private List<Sprite> mirageSprites;
    [SerializeField] private STile stile;
    private void OnEnable()
    {
        DesertArtifact.MoveMadeOnArtifact += MoveMadeOnArtifact;
        mirageIslandId = -1;
    }
    private void OnDisable()
    {
        DesertArtifact.MoveMadeOnArtifact -= MoveMadeOnArtifact;
    }
    public void MirageOnClick()
    {
        if (mirageIslandId != 0)
        {
            DisableMirage();
            return;
        }
        button.SelectButton();
    }
    private void MoveMadeOnArtifact(object sender, System.EventArgs e)
    {
        if (!SaveSystem.Current.GetBool("desertMirage")) return; //Maybe add a collectible check?
        DisableMirage();
        (int, int) cords = (button.x, button.y);
        DesertArtifact artifact = (DesertArtifact)UIArtifact.GetInstance();
        mirageIslandId = artifact.currGrid[cords];
        //Do some sanity checking on mirageID
        Debug.Log($"mirage: {mirageIslandId} (x,y): {cords}");
        if (mirageIslandId < 8) 
        {
            button.SetEmptySprite(mirageSprites[mirageIslandId - 1]);
            button.buttonAnimator.sliderImage.sprite = mirageSprites[mirageIslandId - 1];
            stile.sliderCollider.isTrigger = true;
        }
        //Enable STile
        MirageSTileManager.GetInstance().EnableMirage(mirageIslandId, cords.Item1, cords.Item2);
    }

    private void DisableMirage()
    {
        button.RestoreDefaultEmptySprite();
        button.SetSpriteToIslandOrEmpty();
        MirageSTileManager.GetInstance().DisableMirage(mirageIslandId);
        mirageIslandId = -1;
        stile.sliderCollider.isTrigger = false;
    }
/*    private void OnDisable()
    {
        mirageSTile.DisableMirage();
        mirageIslandId = 0;
        button.SetSpriteToIslandOrEmpty();
    }*/
}
