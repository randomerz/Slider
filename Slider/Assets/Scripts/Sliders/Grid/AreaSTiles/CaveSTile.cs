using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveSTile : STile
{
    public bool isLit
    {
        get;
        private set;
    }

    //L: For Later
    public Shader paletteSwapShader;

    public CaveGrid grid;

    private CaveLight cLight;

    public void Start()
    {
        isLit = false;

        grid = SGrid.current as CaveGrid;

        CaveGrid.OnLightMapUpdate += OnLightMapUpdate;
        SGridAnimator.OnSTileMove += OnTileMoved;

        cLight = GetComponent<CaveLight>();
    }

    private void UpdateIsLit()
    {
        isLit = (SGrid.current as CaveGrid).GetLit(this.x, this.y);
        //Debug.Log("Tile" + this.islandId + " is " + isLit);

        //L: TODO Do something to the shader to update the tiles
    }

    //L: The two ways that a tile could change lighting are if the light map changes or if the tile moves

    public void OnLightMapUpdate(object sender, CaveGrid.OnLightMapUpdateArgs e)
    {
        UpdateIsLit();
    }

    public void OnTileMoved(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile == this)
        {
            UpdateIsLit();
        }
    }
}
