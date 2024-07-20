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
        
        DefaultFactoryPast,
    }

    // Managed by SceneChanger.cs
    public static SpawnLocation nextSpawn;
    // Used by CheatsControlPanel.cs
    public static SpawnLocation lastSpawn;
    public static Area lastArea = Area.None;
    public static Vector3 relativePos;

    public SpawnLocation spawnName;
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

            lastSpawn = nextSpawn;
            nextSpawn = SpawnLocation.Default;
        }

        if (spawnName == SpawnLocation.Default)
        {
            SGrid.Current.DefaultSpawn = this;
            Vector3 pp = GameObject.Find("Player").transform.position;
            if (pp != transform.position && nextSpawn == SpawnLocation.Default && lastSpawn == SpawnLocation.Default)
            {
                Debug.LogWarning($"Default Spawn position was not the same as the player's position. Player: {pp}, spawn: {transform.position}");
            }
        }

        if (spawnName == SpawnLocation.DefaultFactoryPast)
        {
            (SGrid.Current as FactoryGrid).DefaultSpawnFactoryPast = this;
        }
    }

    private void OnEnable()
    {
        QuitHandler.OnQuit += ClearStatics;
    }

    private void OnDisable()
    {
        QuitHandler.OnQuit -= ClearStatics;
    }

    private void ClearStatics(object sender, System.EventArgs e)
    {
        nextSpawn = SpawnLocation.Default;
        lastSpawn = SpawnLocation.Default;
    }

    // Used by CheatsControlPanel.cs
    public void TeleportPlayerToSpawn()
    {
        Player.SetPosition(transform.position);
        Player.SetIsInHouse(transform.position.y <= Player.houseYThreshold);

        if (spawnInBoat)
        {
            wlcManager.SetOnWater(true);
        }
        else
        {
            wlcManager?.SetOnWater(false);
        }

    }
}
