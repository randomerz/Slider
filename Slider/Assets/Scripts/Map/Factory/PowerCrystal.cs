using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCrystal : MonoBehaviour
{
    private const string lightShaderName = "Shader Graphs/LightShaderBoolean";

    private ElectricalNode[] allNodes;
    private Conveyor[] conveyors;
    private List<Material> lightMaterials;

    public static bool Blackout { get; private set; }

    private void Start()
    {
        Blackout = false;
        allNodes = GameObject.FindObjectsOfType<ElectricalNode>();
        conveyors = GameObject.FindObjectsOfType<Conveyor>();

        lightMaterials = FindMaterials();
    }

    public void StartCrystalPoweredSequence()
    {
        AudioManager.StopMusic("Factory");
        StartCoroutine(CrystalPoweredBuildup());
    }

    private IEnumerator CrystalPoweredBuildup()
    {
        yield return new WaitForSeconds(2.0f);
        DoBlackout();
    }

    private void DoBlackout()
    {
        Blackout = true;
        AudioManager.PlayWithVolume("Power Off", 1.0f);
        foreach (var node in allNodes)
        {
            if (node.nodeType == ElectricalNode.NodeType.INPUT)
            {
                node.StartSignal(false);
            }
        }

        foreach (var conveyor in conveyors)
        {
            conveyor.ConveyorEnabled = false;
        }

        foreach (var mat in lightMaterials)
        {
            mat.SetInt("_LightOn", 0);  //L: No set bool? Ya serious?
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
