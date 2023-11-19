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
            DisableMirage();
            return;
        }
        //Debug.Log("selecting");
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
        //Debug.Log($"mirage: {mirageIslandId} (x,y): {cords}");
        if (mirageIslandId < 8) 
        {
            button.SetEmptySprite(mirageSprites[mirageIslandId - 1]);
            button.SetIslandSprite(mirageSprites[mirageIslandId - 1]);
            button.buttonAnimator.sliderImage.sprite = mirageSprites[mirageIslandId - 1]; //I honestly dunno why this line is needed
            stile.sliderCollider.isTrigger = true;
            buttonMirage.SetMirageEnabled(true);
        }
        //Enable STile
        MirageSTileManager.GetInstance().EnableMirage(mirageIslandId, cords.Item1, cords.Item2);
    }

    private void DisableMirage()
    {
        button.RestoreDefaultEmptySprite();
        button.RestoreDefaultIslandSprite();
        button.SetSpriteToIslandOrEmpty();
        MirageSTileManager.GetInstance().DisableMirage(mirageIslandId);
        mirageIslandId = 0;
        stile.sliderCollider.isTrigger = false;
        buttonMirage.SetMirageEnabled(false);

        DesertArtifact.MirageDisappeared?.Invoke(this, new System.EventArgs());
    }
/*    private void OnDisable()
    {
        mirageSTile.DisableMirage();
        mirageIslandId = 0;
        button.SetSpriteToIslandOrEmpty();
    }*/
}
