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
        JungleMinecartWest,
        JungleMinecartEast,

        // Desert
        DesertBridge,
        DesertLava, // not going to be implemented?
        DesertPortal,

        // Factory
        FactoryWest,
        FactoryMinecartWest,
        FactorySouth,
        FactoryNorth,

        // Military
        MilitaryNorth,
        MiliaryEast,

        // Mountain
        MountainSouth,
        MountainSouthMinecart,
        MountainLava, // not going to be implemented?

        // MagiTech
        MagiTechSouth,
        MagiTechDesertPortal,
        MagiTechRocket,

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
