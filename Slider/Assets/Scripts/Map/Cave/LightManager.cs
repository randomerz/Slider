using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LightManager : MonoBehaviour
{
    public CaveLight[] lights;

    public GameObject tilesRoot;
    public GameObject worldBorderTiles;

    private int maskOffsetX;
    private int maskOffsetY;
    private int maskSizeX;
    private int maskSizeY;
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

        maskOffsetX = worldBorderTiles.GetComponent<Tilemap>().cellBounds.x;
        maskOffsetY = worldBorderTiles.GetComponent<Tilemap>().cellBounds.y;
        maskSizeX = worldBorderTiles.GetComponent<Tilemap>().cellBounds.xMax - maskOffsetX;
        maskSizeY = worldBorderTiles.GetComponent<Tilemap>().cellBounds.yMax - maskOffsetY;
        GenerateLightMask();
        UpdateMaterials();
    }

    void GenerateLightMask()
    {
        lightMask = new Texture2D(maskSizeX, maskSizeY);
        for (int x = 0; x < maskSizeX; x++)
        {
            for (int y = 0; y < maskSizeY; y++)
            {
                if (x > maskSizeX / 2)
                {
                    lightMask.SetPixel(x, y, Color.white);
                } else
                {
                    lightMask.SetPixel(x, y, Color.black);
                }

            }
        }

        
        foreach (CaveLight l in lights)
        {
            Texture2D mask = l.GetLightMask(maskOffsetX, maskOffsetY, maskSizeX, maskSizeY);

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
            m.SetVector("_MaskOffset", new Vector4(maskOffsetX, maskOffsetY));
            m.SetVector("_MaskSize", new Vector4(maskSizeX, maskSizeY));
        }
    }

    /* Old Shader
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
