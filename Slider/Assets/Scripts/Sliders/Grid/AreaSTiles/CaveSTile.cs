using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveSTile : STile
{

    public CaveGrid grid;
    public Tilemap wallsSTilemap;

    public List<GameObject> objectsThatBlockLight;

    private new void Awake()
    {
        base.Awake();

        objectsThatBlockLight = new List<GameObject>();
        Transform[] objects = GetComponentsInChildren<Transform>();
        foreach (var o in objects)
        {
            if (o.CompareTag("BlocksLight"))
            {
                objectsThatBlockLight.Add(o.gameObject);
            }
        }
    }

    private void Start()
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
                //Position relative to the center of the tile
                Vector2Int posOnTile = new Vector2Int((int) (go.transform.position.x - transform.position.x), (int) (go.transform.position.y - transform.position.y));
                if (posOnTile.x < -offset || posOnTile.x > offset || posOnTile.y < -offset || posOnTile.y > offset)
                {
                    Debug.LogError("Positions when calculating height mask fall outside the tile's bounds");
                }
                heightMask.SetPixel(posOnTile.x + offset, posOnTile.y + offset, Color.white);
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
