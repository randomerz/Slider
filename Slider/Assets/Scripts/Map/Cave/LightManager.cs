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
}
