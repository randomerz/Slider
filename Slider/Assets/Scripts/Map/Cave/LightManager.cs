using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LightManager : MonoBehaviour
{
    public CaveLight[] lights;

    public GameObject tilesRoot;

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

        UpdateShaderLights();
    }

    void UpdateShaderLights()
    {
        Texture2D lightData = new Texture2D(lights.Length, 4);
        for (int u=0; u<lights.Length; u++)
        {
            Vector4 lightPos = lights[u].transform.position;
            Vector4 lightDir = new Vector4(lights[u].lightDir.x, lights[u].lightDir.y);
            float lightRadius = lights[u].lightRadius;
            float lightArcAngle = lights[u].lightArcAngle;
            lightData.SetPixel(u, 0, lightPos);
            lightData.SetPixel(u, 1, lightDir);
            lightData.SetPixel(u, 2, new Vector4(lightRadius, lightRadius));
            lightData.SetPixel(u, 3, new Vector4(lightArcAngle, lightArcAngle));
        }

        foreach (Material m in caveLightMaterials)
        {
            m.SetTexture("_LightData", lightData);
        }
    }
}
