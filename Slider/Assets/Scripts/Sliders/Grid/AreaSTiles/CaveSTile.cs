using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveSTile : STile
{

    public CaveGrid grid;
    public Tilemap wallsSTilemap;
    public List<Tilemap> objectsThatBlockLight;

    public void Start()
    {

        grid = SGrid.current as CaveGrid;
        
        SGridAnimator.OnSTileMove += UpdateLightMaskAfterMove;

        if (LightManager.instance != null)
        {
            LightManager.instance.FindMaterials();
            LightManager.instance.UpdateAll();
        }

        Tilemap[] tilemapChildren = GetComponentsInChildren<Tilemap>();
        objectsThatBlockLight = new List<Tilemap>();
        foreach (Tilemap tm in tilemapChildren)
        {
            if (tm.CompareTag("BlocksLight"))
            {
                objectsThatBlockLight.Add(tm);
            }
        }
        
    }

    //L: Gets the STILE_WIDTH x STILE_WIDTH (17 x 17) height mask. (1 if there's a wall tile, 0 if not)
    public Texture2D GetHeightMask()
    {
        int offset = STILE_WIDTH / 2;
        Texture2D heightMask = new Texture2D(STILE_WIDTH, STILE_WIDTH);

        //L : Coordinates coorespond to the actual tile coordinates in the world, which are offset from the Texture2D coords by STILE_WIDTH / 2
        /*
        for (int x = -offset; x <= offset; x++)
        {
            for (int y = -offset; y <= offset; y++)
            {
                TileBase tile = wallsSTilemap.GetTile(new Vector3Int(x, y, 0));
                heightMask.SetPixel(x + offset, y + offset, tile != null ? Color.white : Color.black);
            }
        }
        */

        //L : Coordinates coorespond to the actual tile coordinates in the world, which are offset from the Texture2D coords by STILE_WIDTH / 2
        
        foreach (var go in objectsThatBlockLight)
        {
            Tilemap tm = go.GetComponent<Tilemap>();
            if (tm != null)
            {
                for (int x = -offset; x <= offset; x++)
                {
                    for (int y = -offset; y <= offset; y++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        heightMask.SetPixel(x + offset, y + offset, tile != null ? Color.white : Color.black);
                    }
                }

            } else
            {
                Debug.Log("Else?");
                Vector3Int pos = new Vector3Int((int) transform.position.x, (int) transform.position.y, (int) transform.position.z);
                heightMask.SetPixel(x + offset, y + offset, Color.white);
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
