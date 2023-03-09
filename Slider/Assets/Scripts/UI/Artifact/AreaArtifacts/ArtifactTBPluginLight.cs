using UnityEngine;


public class ArtifactTBPluginLight : ArtifactTBPlugin
{
    public bool isLit = true;

    [SerializeField]
    private Sprite islandDarkSprite;
    [SerializeField]
    private Sprite islandLitSprite;

    private void Start() 
    {
        SetLit(isLit);
    }

    public void SetLit(bool isLit)
    {
        this.isLit = isLit;
        if (button.MyStile != null && button.TileIsActive)
        {
            button.SetIslandSprite(isLit ? islandLitSprite : islandDarkSprite);
            button.SetSpriteToIslandOrEmpty();
        }
    }
}