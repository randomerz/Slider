using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    public GameObject[] fishSpawners;
    public static bool fishOn; //temporary
    private bool fishActive;

    void Awake()
    {
        SetFish(false);
    }
    
    void Update()
    {
        // check fish
        if (!fishActive && fishOn)
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
