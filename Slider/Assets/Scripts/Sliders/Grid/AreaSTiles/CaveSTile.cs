using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveSTile : STile
{

    public CaveGrid grid;
    public Tilemap wallsSTilemap;

    public void Start()
    {

        grid = SGrid.current as CaveGrid;
        
        SGridAnimator.OnSTileMove += UpdateHeightMapAfterMove;

    }

    //L: Gets the STILE_WIDTH x STILE_WIDTH (17 x 17) height mask. (1 if there's a wall tile, 0 if not)
    public Texture2D GetHeightMask()
    {
        int offset = STILE_WIDTH / 2;
        Texture2D heightMask = new Texture2D(STILE_WIDTH, STILE_WIDTH);

        //L : Coordinates coorespond to the actual tile coordinates in the world, which are offset from the Texture2D coords by STILE_WIDTH / 2
        for (int x = - STILE_WIDTH / 2; x < STILE_WIDTH / 2; x++)
        {
            for (int y = -STILE_WIDTH / 2; y < STILE_WIDTH / 2; y++)
            {
                heightMask.SetPixel(x + offset, y + offset, wallsSTilemap.GetTile(new Vector3Int(x, y, 0)) != null ? Color.white : Color.black);
            }
        }

        heightMask.Apply();
        return heightMask;
    }

    //L: The two ways that a tile could change lighting are if the light map changes or if the tile moves

    public void UpdateHeightMapAfterMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile == this)
        {

        }
    }
}
