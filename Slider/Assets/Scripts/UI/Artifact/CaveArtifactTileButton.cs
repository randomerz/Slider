using UnityEngine;


class CaveArtifactTileButton : ArtifactTileButton
{
    public bool isLit = true;

    [SerializeField]
    private Sprite islandDarkSprite;
    [SerializeField]
    private Sprite islandLitSprite;

    private void Start()
    {
        base.Start();

        CheckLit();
    }

    private void OnEnable()
    {
        SGrid.OnSTileEnabled += STileEnabled;
        SGrid.OnGridMove += GridMove;
    }

    private void OnDisable()
    {
        base.OnDisable();
        SGrid.OnSTileEnabled -= STileEnabled;
        SGrid.OnGridMove -= GridMove;
    }

    private void STileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        CheckLit();
    }

    private void GridMove(object sender, SGrid.OnGridMoveArgs e)
    {
        CheckLit();
    }

    public void CheckLit()
    {
        isLit = (myStile as CaveSTile).GetTileLit();
        islandSprite = isLit ? islandLitSprite : islandDarkSprite;
    }
}

