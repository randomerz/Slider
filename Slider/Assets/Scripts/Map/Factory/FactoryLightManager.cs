using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryLightManager : Singleton<FactoryLightManager>
{
    private const string lightShaderName = "Shader Graphs/FactoryLightShader";
    private const string pipeShaderName = "Shader Graphs/FactoryLightColorSwapShader";

    //RIP SR

    [SerializeField] private List<Material> lightMaterials;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        lightMaterials = FindMaterials();
    }

    public static void SwitchLights(bool on)
    {
        foreach (var mat in _instance.lightMaterials)
        {
            mat.SetInt("_LightOn", on ? 1 : 0);
        }
    }

    private List<Material> FindMaterials()
    {
        List<Material> materials = new List<Material>();
        Shader lightShader = Shader.Find(lightShaderName);
        Shader pipeShader = Shader.Find(pipeShaderName);

        Renderer[] allRenderers = FindObjectsOfType<Renderer>(true);
        foreach (Renderer r in allRenderers)
        {
            if (r.material.shader == lightShader || r.material.shader == pipeShader)
            {
                materials.Add(r.material);
            }
        }

        return materials;
    }
}
