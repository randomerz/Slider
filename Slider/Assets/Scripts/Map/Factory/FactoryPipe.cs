using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryPipe : MonoBehaviour
{
    private const string pipeShaderName = "Shader Graphs/FactoryLightColorSwapShader";

    public Material pipeMat;
    private Color gooColor = new Color(192f/255f, 96f/255f, 192f/255f);
    private List<Material> materials;
    private bool didInit;

    private void FindMaterials()
    {
        materials = new List<Material>();
        Shader pipeShader = Shader.Find(pipeShaderName);

        Renderer[] allRenderers = FindObjectsOfType<Renderer>(true);
        foreach (Renderer r in allRenderers)
        {
            if (r.material.shader == pipeShader)
            {
                materials.Add(r.material);
            }
        }
        didInit = true;
    }

    public void ActivateGoo()
    {
        if(!didInit)
            FindMaterials();

        foreach (var mat in materials)
        {
            mat.SetColor("_ReplacementColor", gooColor);
        }
    }

    public void DeActivateGoo()
    {
        if(!didInit)
            FindMaterials();

        foreach (var mat in materials)
        {
            mat.SetColor("_ReplacementColor", GameSettings.black);
        }
    }
}
