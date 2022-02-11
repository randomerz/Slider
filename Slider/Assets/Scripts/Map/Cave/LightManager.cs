using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/* L : Responsible for storing heightMask, lightMask, and updating materials based on the lightMask. */
public class LightManager : MonoBehaviour
{
    public CaveLight[] lights;
    public CaveSTile[] stiles;

    public GameObject tilesRoot;
    public Tilemap worldWallsTilemap;
    public Tilemap worldBorderColliderTilemap;

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

    void Start()
    {
        Renderer[] renderers = tilesRoot.GetComponentsInChildren<Renderer>();
        caveLightMaterials = new List<Material>();
        caveShader = Shader.Find("Shader Graphs/CaveTileLightShader");
        Debug.Log(caveShader);
        foreach (Renderer r in renderers)
        {
            if (r.material.shader == caveShader)
            {
                caveLightMaterials.Add(r.material);
                //Debug.Log(r.gameObject.name);
            }
        }

        worldToMaskDX = -worldBorderColliderTilemap.cellBounds.x;
        worldToMaskDY = -worldBorderColliderTilemap.cellBounds.y;
        maskSizeX = worldBorderColliderTilemap.cellBounds.xMax + worldToMaskDX;
        maskSizeY = worldBorderColliderTilemap.cellBounds.yMax + worldToMaskDY;
        GenerateHeightMask();
        GenerateLightMask();
        UpdateMaterials();
    }

    void GenerateHeightMask()
    {
        heightMask = new Texture2D(maskSizeX, maskSizeY);
        for (int u = 0; u < maskSizeX; u++)
        {
            for (int v = 0; v < maskSizeY; v++)
            {
                heightMask.SetPixel(u, v, worldWallsTilemap.GetTile(new Vector3Int(u - worldToMaskDX, v - worldToMaskDY, 0)) != null ? Color.white : Color.black);
            }
        }

        foreach (CaveSTile stile in stiles)
        {
            Texture2D mask = stile.GetHeightMask();
            //L: Convert from the stile mask coords to the overall texture coords.
            for (int u1 = 0; u1 < stile.STILE_WIDTH; u1++)
            {
                for (int v1 = 0; v1 < stile.STILE_WIDTH; v1++)
                {
                    int u2 = u1 - stile.STILE_WIDTH / 2 + (int) stile.transform.position.x + worldToMaskDX;
                    int v2 = v1 - stile.STILE_WIDTH / 2 + (int)stile.transform.position.x + worldToMaskDY;

                    heightMask.SetPixel(u2, v2, mask.GetPixel(u1, v1));
                }
            }
        }

        heightMask.Apply();
    }

    void GenerateLightMask()
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
            Texture2D mask = l.GetLightMask(heightMask, worldToMaskDX, worldToMaskDY, maskSizeX, maskSizeY);

            //L: Add the pixels together
            for (int x = 0; x < maskSizeX; x++)
            {
                for (int y = 0; y < maskSizeY; y++)
                {
                    Color ogPixel = lightMask.GetPixel(x, y); ;
                    Color newPixel = mask.GetPixel(x, y);
                    lightMask.SetPixel(x, y, ogPixel + newPixel);
                }
            }
        }
        

        lightMask.Apply();
    }

    void UpdateMaterials()
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
