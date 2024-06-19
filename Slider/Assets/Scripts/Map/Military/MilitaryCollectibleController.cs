using System.Collections.Generic;
using UnityEngine;

public class MilitaryCollectibleController : Singleton<MilitaryCollectibleController>
{
    public GameObject collectiblePrefab;

    // Tile 1 is always spawned by default
    // Tile 16 has four walls and no unit available. always spawned last? can move it earlier if game is too easy
    // private int[] spawnedTileIdsOrder = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
    private int[] oddTileOrder = new int[] { 3, 5, 7, 9, 13, 15 };
    private int[] evenTileOrder = new int[] { 2, 4, 6, 8, 12, 14 };
    private int numSpawned = 1;

    private void Awake()
    {
        InitializeSingleton();
        Reset();
    }

    public static void Reset()
    {
        // Shuffle order
        for (int i = 0; i < _instance.oddTileOrder.Length; i += 1)
        {
            int r = Random.Range(i, _instance.oddTileOrder.Length);
            (_instance.oddTileOrder[i], _instance.oddTileOrder[r]) = (_instance.oddTileOrder[r], _instance.oddTileOrder[i]);
        }
        for (int i = 0; i < _instance.evenTileOrder.Length; i += 1)
        {
            int r = Random.Range(i, _instance.evenTileOrder.Length);
            (_instance.evenTileOrder[i], _instance.evenTileOrder[r]) = (_instance.evenTileOrder[r], _instance.evenTileOrder[i]);
        }
        _instance.numSpawned = 1;
    }

    public static void SpawnMilitaryCollectible(Transform startTransform, MilitarySTile stile)
    {
        Transform targetTransform = stile.sliderCollectibleSpawn;
        int nextIslandId = _instance.GetNextSpawnedId();

        if (nextIslandId == -1)
        {
            Debug.Log($"Spawned all tiles already");
            return;
        }

        AudioManager.Play("Puzzle Complete");

        GameObject go = Instantiate(_instance.collectiblePrefab, startTransform.position, Quaternion.identity, targetTransform);
        Collectible collectible = go.GetComponent<Collectible>();

        collectible.onCollect.RemoveAllListeners();
        collectible.onCollect.AddListener(() => { collectible.ActivateSTile(nextIslandId); });
        collectible.onCollect.AddListener(() => MilitaryResetChecker.DecrementCollectible());

        // TODO: animate from start transform to end
        collectible.transform.position = targetTransform.position;
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, targetTransform.position, targetTransform);

        UITrackerManager.AddNewTracker(go, sprite: UITrackerManager.DefaultSprites.circle2);
    
        MilitaryResetChecker.IncrementCollectible();
    }

    private int GetNextSpawnedId()
    {
        if (numSpawned > 15)
        {
            // Spawned everything!
            return -1;
        }
        else if (numSpawned == 15)
        {
            numSpawned += 1;
            return 10; // last supply drop + last tile
        }
        else if (numSpawned == 14)
        {
            numSpawned += 1;
            return 11; // no supply warning
        }
        else if (numSpawned == 13)
        {
            numSpawned += 1;
            return 16; // 4 walls + alien npcs tile
        }

        int nextSpawn;
        if (numSpawned % 2 == 0)
        {
            nextSpawn = oddTileOrder[(numSpawned - 2) / 2];
        }
        else
        {
            nextSpawn = evenTileOrder[(numSpawned - 1) / 2];
        }
        numSpawned += 1;
        return nextSpawn;
    }
}