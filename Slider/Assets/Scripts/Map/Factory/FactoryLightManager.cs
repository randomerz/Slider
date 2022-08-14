using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryLightManager : Singleton<FactoryLightManager>
{
    private const string lightShaderName = "Shader Graphs/LightShaderBoolean";

    [SerializeField] private SpriteRenderer playerSR;   //L: Not the kind of SR I'm used to.

    private List<Material> lightMaterials;

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
            mat.SetInt("_LightOn", on ? 1 : 0);  //L: No set bool? Ya serious?
        }
    }

    private List<Material> FindMaterials()
    {
        List<Material> materials = new List<Material>();
        Shader lightShader = Shader.Find(lightShaderName);

        Renderer[] allRenderers = FindObjectsOfType<Renderer>(true);
        foreach (Renderer r in allRenderers)
        {
            if (r.material.shader == lightShader)
            {
                materials.Add(r.material);
                //Debug.Log(r.gameObject.name);
            }
        }

        return materials;
    }
}
