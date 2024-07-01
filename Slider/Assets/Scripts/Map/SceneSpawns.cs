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
        MiliaryWest,

        // Mountain
        MountainSouth,
        MountainSouthMinecart,
        MountainLava, // not going to be implemented?

        // MagiTech
        MagiTechSouth,
        MagiTechDesertPortal,
        MagiTechRocket,

        // New
        FactoryPastNorth,
        MagiTechPastSouth,

        JungleNorthRiver,
        DesertRiver,

        // Space
    }

    // Managed by SceneChanger.cs
    public static SpawnLocation nextSpawn;
    public static Area lastArea = Area.None;
    public static Vector3 relativePos;

    [SerializeField] private SpawnLocation spawnName;
    [SerializeField] private bool spawnInBoat;
    [SerializeField] private WaterLandColliderManager wlcManager;



    void Awake()
    {
        if (nextSpawn == spawnName && nextSpawn != SpawnLocation.Default) 
        {
            Vector3 pos = transform.position + relativePos;
            // Debug.Log("relative pos:" + relativePos);
            relativePos = Vector3.zero;
            GameObject.Find("Player").transform.position = pos;

            if (spawnInBoat)
            {
                wlcManager.SetOnWater(true);
            }
            else
            {
                wlcManager?.SetOnWater(false);
            }

            nextSpawn = SpawnLocation.Default;
        }
    }
}
