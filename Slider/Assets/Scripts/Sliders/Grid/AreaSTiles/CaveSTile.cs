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
        
        SGridAnimator.OnSTileMove += UpdateLightMaskAfterMove;

        if (LightManager.instance != null)
        {
            LightManager.instance.FindMaterials();
            LightManager.instance.UpdateAll();
        }
    }

    //L: Gets the STILE_WIDTH x STILE_WIDTH (17 x 17) height mask. (1 if there's a wall tile, 0 if not)
    public Texture2D GetHeightMask()
    {
        int offset = STILE_WIDTH / 2;
        Texture2D heightMask = new Texture2D(STILE_WIDTH, STILE_WIDTH);

        //L : Coordinates coorespond to the actual tile coordinates in the world, which are offset from the Texture2D coords by STILE_WIDTH / 2
        for (int x = - STILE_WIDTH / 2; x <= STILE_WIDTH / 2; x++)
        {
            for (int y = -STILE_WIDTH / 2; y <= STILE_WIDTH / 2; y++)
            {
                TileBase tile = wallsSTilemap.GetTile(new Vector3Int(x, y, 0));
                heightMask.SetPixel(x + offset, y + offset, tile != null ? Color.white : Color.black);
            }
        }

        heightMask.Apply();
        return heightMask;
    }

    //L: The two ways that a tile could change lighting are if the light map changes or if the tile moves

    public void UpdateLightMaskAfterMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile == this)
        {
            /*
            LightManager.instance.ClearHeightMaskTile(e.prevPos);
            LightManager.instance.UpdateHeightMask(this);
            

            LightManager.instance.GenerateLightMask();  //L: Regenerate the whole mask cuz I'm lazy
            LightManager.instance.UpdateMaterials();
            */

            LightManager.instance.UpdateAll();
        }
    }
}
