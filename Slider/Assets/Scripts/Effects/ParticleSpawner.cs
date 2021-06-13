using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    public GameObject[] fishSpawners;
    private bool fishActive;

    void Awake()
    {
        SetFish(false);
    }
    
    void Update()
    {
        // check fish
        if (!fishActive && NPCManager.fishOn)
        {
            SetFish(true);
        }
    }

    private void SetFish(bool v)
    {
        fishActive = v;
        foreach (GameObject g in fishSpawners)
        {
            g.SetActive(v);
        }
    }
}
