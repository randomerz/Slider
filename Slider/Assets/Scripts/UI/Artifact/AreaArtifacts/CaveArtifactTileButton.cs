using UnityEngine;


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
        UIArtifact.OnButtonInteract += ButtonDid;
        CaveLight.OnLightSwitched += LightSwitched;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        SGrid.OnSTileEnabled -= STileEnabled;
        UIArtifact.OnButtonInteract -= ButtonDid;
        CaveLight.OnLightSwitched -= LightSwitched;
    }

    private void STileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        CheckLit();
    }

    private void ButtonDid(object sender, System.EventArgs e)
    {
        CheckLit();
    }

    private void LightSwitched(object sender, CaveLight.OnLightSwitchedArgs e)
    {
        Debug.Log("YEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        CheckLit();
    }

    public void CheckLit()
    {
        if (myStile != null && isTileActive)
        {
            isLit = (myStile as CaveSTile).GetTileLit(this.x, this.y);
            islandSprite = isLit ? islandLitSprite : islandDarkSprite;
            ResetToIslandSprite();
        }
    }
}

