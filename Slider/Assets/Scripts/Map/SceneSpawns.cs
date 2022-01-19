using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSpawns : MonoBehaviour
{
    public static string nextSpawn;

    [SerializeField] private string spawnName;
    [SerializeField] private bool spawnInBoat;

    void Start()
    {
        if (nextSpawn == spawnName) 
        {
            Player.SetPosition(transform.position);
        }
    }
}
