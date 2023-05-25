using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveSTile : STile
{
    [Header("Cave STile")]
    public CaveGrid grid;
    public Tilemap wallsSTilemap;

    public List<Vector2Int> validDirsForLight;

    [SerializeField] private List<CaveLight> borderLights;

    private List<GameObject> objectsThatBlockLight;

    private Color[] _heightMask;
    public Color[] HeightMask
    {
        get
        {
            if (_heightMask == null)
            {
                GenerateHeightMask();
            }
            return _heightMask;
        }
    }

    private new void Awake()
    {
        base.Awake();

        objectsThatBlockLight = new List<GameObject>();
        Transform[] objects = GetComponentsInChildren<Transform>(true); // true -> include inactive components
        foreach (var o in objects)
        {
            if (o.CompareTag("BlocksLight") && o.gameObject.activeSelf)
            {
                objectsThatBlockLight.Add(o.gameObject);
            }
        }
    }

    private new void Start()
    {
        grid = SGrid.Current as CaveGrid;

        base.Start();
    }

    public override void SetTileActive(bool isTileActive)
    {
        base.SetTileActive(isTileActive);
        
        //if (isTileActive && LightManager.instance != null)
        //{
        //    LightManager.instance.UpdateHeightMask(this);
        //    LightManager.instance.UpdateMaterials();
        //}
    }

    public bool GetTileLit(int x, int y)
    {
        //Check if this tile has a light source
        CaveLight thisLight = GetComponentInChildren<CaveLight>();
        if (thisLight != null && thisLight.LightOn)
        {
            return true;
        }

        //Check if any valid adjacent tile has a light source
        foreach (var dir in validDirsForLight)
        {
            Vector2Int posToCheck = new Vector2Int(x, y) + dir;
            //Border lights (hardcoded for now)
            foreach (CaveLight light in borderLights) {
                if (light != null && light.LightOn && light.GetPos() == posToCheck)
                {
                    return true;
                } 
            }

            //Check position is in the grid
            if (posToCheck.x >= 0 && posToCheck.x < SGrid.Current.Width && posToCheck.y >= 0 && posToCheck.y < SGrid.Current.Height)
            {
                CaveSTile tile = (CaveSTile)SGrid.Current.GetStileAt(posToCheck.x, posToCheck.y);
                CaveLight light = tile.GetComponentInChildren<CaveLight>();
                if (tile.isTileActive && light != null && light.LightOn)
                {
                    foreach (var lightDir in tile.validDirsForLight)
                    {
                        //The light needs to be able to exit the tile with the light AND enter the tile we are checking from that direction.
                        if (lightDir == -dir)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    public bool GetTileLit()
    {
        return GetTileLit(this.x, this.y);
    }

    //L: Gets the STILE_WIDTH x STILE_WIDTH (17 x 17) height mask. (1 if there's a wall tile, 0 if not)
    private void GenerateHeightMask()
    {
        int offset = STILE_WIDTH / 2;
        _heightMask = new Color[STILE_WIDTH * STILE_WIDTH];

        for (int i = 0; i < STILE_WIDTH*STILE_WIDTH; i++)
        {
            _heightMask[y*STILE_WIDTH+x] = Color.black;
        }
        
        foreach (var go in objectsThatBlockLight)
        {
            Tilemap tm = go.GetComponent<Tilemap>();
            if (tm != null)
            {
                BoundsInt bounds = new BoundsInt(-offset, -offset, 0, STILE_WIDTH, STILE_WIDTH, 1);
                foreach (var pos in tm.cellBounds.allPositionsWithin)
                {
                    TileBase tile = tm.GetTile(pos);
                    if (tile != null)
                    {
                        _heightMask[(pos.y+offset)*STILE_WIDTH + (pos.x+offset)] = Color.white;
                    }
                }
            } 
            else
            {
                //Position relative to the center of the tile
                int centerX = (int) (go.transform.position.x - transform.position.x);
                int centerY = (int) (go.transform.position.y - transform.position.y);

                if (centerX < -offset || centerX > offset || centerY < -offset || centerY > offset)
                {
                    Debug.LogError("Positions when calculating height mask fall outside the tile's bounds");
                }

                _heightMask[(centerY + offset) * STILE_WIDTH + (centerX + offset)] = Color.white;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        //DrawHeightMask(); //Enable for debugging
    }

    private void DrawHeightMask()
    {
        if (_heightMask != null)
        {
            Gizmos.color = Color.white;
            for (int x = 0; x < STILE_WIDTH; x++)
            {
                for (int y = 0; y < STILE_WIDTH; y++)
                {
                    if (_heightMask[y * STILE_WIDTH + x] == Color.white)
                    {
                        Vector3 worldPos = new Vector3(x + transform.position.x - STILE_WIDTH / 2, y + transform.position.y - STILE_WIDTH / 2, 0);
                        Gizmos.DrawSphere(worldPos, 0.2f);
                    }
                }
            }
        }
    }
}
