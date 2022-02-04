using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveSTile : STile
{
    private bool isShadowed;

    //L: For Later
    public Shader paletteSwapShader;

    public CaveGrid grid;

    private CaveLight light;

    public void Start()
    {
        isShadowed = false;

        grid = SGrid.current as CaveGrid;

        SGridAnimator.OnSTileMove += UpdateMapAndShadowsAfterMove;
        CaveGrid.OnLightMapUpdate += UpdateIsShadowedHandler;

        light = GetComponent<CaveLight>();
    }

    public void UpdateIsShadowed()
    {
        isShadowed = (SGrid.current as CaveGrid).GetLit(this.x, this.y);

        //L: TODO Do something to the shader to update the tiles
    }

    public void UpdateIsShadowedHandler(object sender, CaveGrid.OnLightMapUpdateArgs e)
    {
        isShadowed = e.lightMap[x, y];
    }

    private void UpdateMapAndShadowsAfterMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile == this)
        {
            UpdateIsShadowed();
        }

        if (light != null)
        {
            grid.SetLit(e.prevPos.x, e.prevPos.y, false);
            grid.SetLit(e.stile.x, e.stile.y, false);
        }
    }
}
