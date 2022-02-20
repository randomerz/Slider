using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    public GameObject[] fishSpawners;
    public void SetFishOn()
    {
        foreach (GameObject g in fishSpawners)
        {
            g.SetActive(true);
        }
    }
}