using UnityEngine;


class ArtifactTBPluginLight : ArtifactTBPlugin
{
    public bool isLit = true;

    [SerializeField]
    private Sprite islandDarkSprite;
    [SerializeField]
    private Sprite islandLitSprite;

    private void OnEnable()
    {
        CheckLit();
        SGrid.OnSTileEnabled += STileEnabled;
        UIArtifact.OnButtonInteract += ButtonInteract;
        CaveLight.OnLightSwitched += LightSwitched;
    }

    private void OnDisable()
    {
        SGrid.OnSTileEnabled -= STileEnabled;
        UIArtifact.OnButtonInteract -= ButtonInteract;
        CaveLight.OnLightSwitched -= LightSwitched;
    }

    private void Start()
    {
        CheckLit();
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
        if (button.MyStile != null && button.TileIsActive)
        {
            isLit = (button.MyStile as CaveSTile).GetTileLit(button.x, button.y);
            button.SetIslandSprite(isLit ? islandLitSprite : islandDarkSprite);
            button.SetSpriteToIslandOrEmpty();
        }
    }
}