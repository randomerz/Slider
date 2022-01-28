using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSpawns : MonoBehaviour
{
    public enum SpawnLocation 
    {
        Default,

        VillageAlcove,
        VillageWaterfall,
        VillageBeach,
        
        CavesSouth,
        CavesWaterfall,
        CavesBridge,
        CavesMinecart,
        CavesNorth,

        OceanWest,
        OceanJungleRiver,
        OceanJungleBushes,
        OceanEast,

        JungleRiver,
        JungleBushes,
        JungleWestBridge,
        JungleNorthBridge,
        JungleEast,

        // Desert

        // don't forget FactoryMinecart

        // Mountain

        // Military

        // MagiTech

        // Space
    }

    public static SpawnLocation nextSpawn;

    [SerializeField] private SpawnLocation spawnName;
    [SerializeField] private bool spawnInBoat;

    void Start()
    {
        if (nextSpawn == spawnName && nextSpawn != SpawnLocation.Default) 
        {
            Player.SetPosition(transform.position);
        }
    }
}
