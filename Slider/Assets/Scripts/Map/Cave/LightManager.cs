using System.Collections;
using System.Collections.Generic;
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
    private List<Material> caveLightMaterials;
    private Shader caveShader;

    public static LightManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        worldToMaskDX = -worldBorderColliderTilemap.cellBounds.x;
        worldToMaskDY = -worldBorderColliderTilemap.cellBounds.y;
        maskSizeX = worldBorderColliderTilemap.cellBounds.xMax + worldToMaskDX;
        maskSizeY = worldBorderColliderTilemap.cellBounds.yMax + worldToMaskDY;

        FindMaterials();
        UpdateAll();
    }

    private void Update() // DC: my cpu is crying L: Crying Emoji
    {
        UpdateAll();
    }

    public void UpdateAll()
    {
        GenerateHeightMask();
        GenerateLightMask();
        UpdateMaterials();
    }

    public void FindMaterials()
    {
        Renderer[] renderers = tilesRoot.GetComponentsInChildren<Renderer>();
        caveLightMaterials = new List<Material>();
        caveShader = Shader.Find("Shader Graphs/CaveTileLightShader");
        foreach (Renderer r in renderers)
        {
            if (r.material.shader == caveShader)
            {
                caveLightMaterials.Add(r.material);
                //Debug.Log(r.gameObject.name);
            }
        }
    }

    public void GenerateHeightMask()
    {
        heightMask = new Texture2D(maskSizeX, maskSizeY);

        // Update mask based on the world walls (don't do anymore)
        for (int u = 0; u < maskSizeX; u++)
        {
            for (int v = 0; v < maskSizeY; v++)
            {
                // heightMask.SetPixel(u, v, Color.black);
                heightMask.SetPixel(u, v, worldLightColliderTilemap.GetTile(new Vector3Int(u - worldToMaskDX, v - worldToMaskDY, 0)) != null ? Color.white : Color.black);
            }
        }

        heightMask.Apply();
        

        foreach (CaveSTile stile in stiles)
        {
            UpdateHeightMask(stile);
        }
    }

    public void UpdateHeightMask(CaveSTile stile)
    {
        if (stile.isTileActive)
        {
            Texture2D mask = stile.GetHeightMask();
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
    }

    public void ClearHeightMaskTile(Vector2Int tilePos)
    {
        for (int u1 = 0; u1 < 17; u1++)
        {
            for (int v1 = 0; v1 < 17; v1++)
            {
                int u2 = u1 - 17 / 2 + (int) tilePos.x + worldToMaskDX;
                int v2 = v1 - 17 / 2 + (int) tilePos.y + worldToMaskDY;

                heightMask.SetPixel(u2, v2, Color.black);
            }
        }

        heightMask.Apply();
    }

    public void GenerateLightMask()
    {
        lightMask = new Texture2D(maskSizeX, maskSizeY);
        //L: Set all pixels to black (default is grey color)
        for (int x = 0; x < maskSizeX; x++)
        {
            for (int y = 0; y < maskSizeY; y++)
            {
                lightMask.SetPixel(x, y, Color.black);
            }
        }

        lightMask.Apply();
        
        foreach (CaveLight l in lights)
        {
            UpdateLightMask(l);
        }
    }

    public void UpdateLightMask(CaveLight l)
    {
        if (l.lightOn)
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

    public bool GetLightMaskAt(int x, int y)
    {
        //Debug.Log(lightMask.GetPixel(x + worldToMaskDX, y + worldToMaskDY));
        return lightMask.GetPixel(x + worldToMaskDX, y + worldToMaskDY).r > 0.5f;
    }

    public bool GetLightMaskAt(Tilemap tm, Vector3Int pos)
    {
        Vector3 posInWorld = tm.CellToWorld(pos);
        Vector3Int tilePos = TileUtil.WorldToTileCoords(posInWorld);
        return GetLightMaskAt(tilePos.x, tilePos.y);
    }

    public void UpdateMaterials()
    {
        foreach (Material m in caveLightMaterials)
        {
            m.SetTexture("_LightMask", lightMask);
            m.SetVector("_MaskOffset", new Vector4(worldToMaskDX, worldToMaskDY));
            m.SetVector("_MaskSize", new Vector4(maskSizeX, maskSizeY));
        }
    }

    /* Old Shader Code
    void UpdateShaderLights()
    {
        Matrix4x4 pos = new Matrix4x4();
        Matrix4x4 dir = new Matrix4x4();
        Vector4 radius = new Vector4();
        Vector4 arcAngle = new Vector4();
        Vector4 active = new Vector4();
        for (int i=0; i<lights.Length; i++)
        {
            pos.SetRow(i, lights[i].transform.position);
            dir.SetRow(i, new Vector4(lights[i].lightDir.x, lights[i].lightDir.y));
            radius[i] = lights[i].lightRadius;
            arcAngle[i] = lights[i].lightArcAngle;
            //Debug.Log(lights[i].enabled);
            Debug.Log(lights[i].lightOn);
            active[i] = lights[i].gameObject.activeInHierarchy && lights[i].lightOn ? 1.0f : 0.0f;
            Debug.Log(active[i]);
        }

        foreach (Material m in caveLightMaterials)
        {
            m.SetMatrix("_LightPos", pos);
            m.SetMatrix("_LightDir", dir);
            m.SetVector("_LightRadius", radius);
            m.SetVector("_LightArcAngle", arcAngle);
            m.SetVector("_LightActive", active);
        }
    }
    */
}
