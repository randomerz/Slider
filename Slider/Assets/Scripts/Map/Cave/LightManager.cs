using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/* L: Responsible for storing heightMask, lightMask, and updating materials based on the lightMask. */
/*L: IMPORTANT THINGS: 
 * - If you use SetPixel on a Texture2D you HAVE to call Apply() or else it won't update
 * - ALWAYS call UpdateMaterials() any time you change the heightMask or lightMask.
*/
public class LightManager : MonoBehaviour
{
    public CaveLight[] lights;
    public CaveSTile[] stiles;

    public GameObject tilesRoot;
    public Tilemap worldBorderColliderTilemap;
    public Tilemap worldLightColliderTilemap;

    private int worldToMaskDX;
    private int worldToMaskDY;
    private int maskSizeX;
    private int maskSizeY;

    [SerializeField]
    private Texture2D heightMask;
    [SerializeField]
    private Texture2D lightMask;

    [SerializeField]
    private List<Material> _caveLightMaterials;

    private Shader caveShader;

    public static LightManager instance;

    public static event System.EventHandler<System.EventArgs> OnLightMaskChanged;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        //if (heightMask == null || lightMask == null)
        //{
            DoLightingPreCalculations();
        //}

        if (_caveLightMaterials == null || _caveLightMaterials.Count == 0)
        {
            FindMaterials();
        }
        UpdateMaterials();
    }

    private void OnEnable()
    {
        SGrid.OnSTileEnabled += UpdateAll;
        SGridAnimator.OnSTileMoveEnd += UpdateAll;
    }

    private void OnDisable()
    {
        SGrid.OnSTileEnabled -= UpdateAll;
        SGridAnimator.OnSTileMoveEnd -= UpdateAll;
    }

    public void UpdateAll()
    {
        GenerateHeightMask();
        UpdateLightMaskAll();
        UpdateMaterials();
    }

    //These are just to handle various events
    public void UpdateAll(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        UpdateAll();
    }

    private void UpdateAll(object sender, SGrid.OnSTileEnabledArgs e)
    {
        UpdateAll();
    }
    

    private void SetTileMapSize()
    {
        worldToMaskDX = -worldBorderColliderTilemap.cellBounds.x;
        worldToMaskDY = -worldBorderColliderTilemap.cellBounds.y;
        maskSizeX = worldBorderColliderTilemap.cellBounds.xMax + worldToMaskDX;
        maskSizeY = worldBorderColliderTilemap.cellBounds.yMax + worldToMaskDY;
    }

    public void DoLightingPreCalculations()
    {
        SetTileMapSize();
        GenerateHeightMask();
        GenerateEmptyLightMask();
        foreach (CaveLight l in lights)
        {
            //L: Need this because of script order.
            if (l.lightOnStart)
            {
                STile lOnTile = l.GetComponentInParent<STile>();
                //Make sure the light is actually there.
                if (lOnTile == null || lOnTile.isTileActive)
                {
                    UpdateLightMask(l);
                }
            }
        }
    }
    private void FindMaterials()
    {
        _caveLightMaterials = new List<Material>();
        caveShader = Shader.Find("Shader Graphs/CaveTileLightShader");

        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        foreach (Renderer r in allRenderers)
        {
            if (r.material.shader == caveShader)
            {
                _caveLightMaterials.Add(r.material);
                //Debug.Log(r.gameObject.name);
            }
        }
    }

    private void GenerateHeightMask()
    {
        heightMask = new Texture2D(maskSizeX, maskSizeY);

        //L: Only ever need to do this part once!
        for (int u = 0; u < maskSizeX; u++)
        {
            for (int v = 0; v < maskSizeY; v++)
            {
                heightMask.SetPixel(u, v, Color.black);
                heightMask.SetPixel(u, v, worldLightColliderTilemap.GetTile(new Vector3Int(u - worldToMaskDX, v - worldToMaskDY, 0)) != null ? Color.white : Color.black);
            }
        }
        heightMask.Apply();
        

        foreach (CaveSTile stile in stiles)
        {
            UpdateHeightMask(stile);
        }
    }

    private void GenerateEmptyLightMask()
    {
        //L: Set all pixels to black (only do this once!)
        lightMask = new Texture2D(maskSizeX, maskSizeY);
        for (int x = 0; x < maskSizeX; x++)
        {
            for (int y = 0; y < maskSizeY; y++)
            {
                lightMask.SetPixel(x, y, Color.black);
            }
        }

        lightMask.Apply();
    }

    public void UpdateLightMaskAll()
    {
        GenerateEmptyLightMask();

        foreach (CaveLight l in lights)
        {
            if (l.LightOn)
            {
                UpdateLightMask(l);
            }
        }

        OnLightMaskChanged?.Invoke(this, new System.EventArgs());
    }

    public void UpdateMaterials()
    {
        foreach (Material m in _caveLightMaterials)
        {
            m.SetTexture("_LightMask", lightMask);
            m.SetVector("_MaskOffset", new Vector4(worldToMaskDX, worldToMaskDY));
            m.SetVector("_MaskSize", new Vector4(maskSizeX, maskSizeY));
        }
    }

    public void UpdateHeightMask(CaveSTile stile)
    {
        if (stile.isTileActive)
        {
            Texture2D mask = stile.HeightMask;
            //L: Convert from the stile mask coords to the overall texture coords.
            for (int u1 = 0; u1 < stile.STILE_WIDTH; u1++)
            {
                for (int v1 = 0; v1 < stile.STILE_WIDTH; v1++)
                {
                    int u2 = u1 - stile.STILE_WIDTH / 2 + (int)stile.transform.position.x + worldToMaskDX;
                    int v2 = v1 - stile.STILE_WIDTH / 2 + (int)stile.transform.position.y + worldToMaskDY;
                    heightMask.SetPixel(u2, v2, mask.GetPixel(u1, v1));
                }
            }

            heightMask.Apply();
        }

        //Light mask needs to update when height mask updates
    }

    public void UpdateLightMask(CaveLight l)
    {
        if (l.LightOn)
        {
            Texture2D mask = l.GetLightMask(heightMask, worldToMaskDX, worldToMaskDY, maskSizeX, maskSizeY);

            //L: Add the pixels together
            for (int x = 0; x < maskSizeX; x++)
            {
                for (int y = 0; y < maskSizeY; y++)
                {
                    Color ogPixel = lightMask.GetPixel(x, y);
                    Color newPixel = mask.GetPixel(x, y);
                    lightMask.SetPixel(x, y, ogPixel + newPixel);
                }
            }
        }

        lightMask.Apply();
    }

    //x and y are in world coordinates.
    public bool GetLightMaskAt(int x, int y)
    {
        //Debug.Log(lightMask.GetPixel(x + worldToMaskDX, y + worldToMaskDY));
        return lightMask.GetPixel(x + worldToMaskDX, y + worldToMaskDY).r > 0.5f;
    }

    //Get the light mask at a tile within a tilemap. pos is in cell coordinates with respect to tm. 
    public bool GetLightMaskAt(Tilemap tm, Vector3Int pos)
    {
        Vector3 posInWorld = tm.CellToWorld(pos);
        Vector2Int tilePos = TileUtil.WorldToTileCoords(posInWorld);
        return GetLightMaskAt(tilePos.x, tilePos.y);
    }

    private void OnDrawGizmosSelected()
    {
        for (int x = -worldToMaskDX; x < maskSizeX - worldToMaskDX; x++)
        {
            for (int y = -worldToMaskDY; y < maskSizeY - worldToMaskDY; y++)
            {
                if (GetLightMaskAt(x, y))
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(new Vector3(x, y, 0), 0.2f);
                }
            }
        }
    }
}