using System.Collections.Generic;
using UnityEngine;

public class MilitaryCollectibleController : Singleton<MilitaryCollectibleController>
{
    public GameObject collectiblePrefab;

    // Tile 1 is always spawned by default
    // Tile 16 has four walls and no unit available. always spawned last? can move it earlier if game is too easy
    private int[] spawnedTileIdsOrder = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
    private int numSpawned = 1;

    private void Awake()
    {
        InitializeSingleton();
        Reset();
    }

    public static void Reset()
    {
        // Shuffle order -- keep first and last
        for (int i = 1; i < 15; i++)
        {
            int r = Random.Range(i, 15);
            (_instance.spawnedTileIdsOrder[i], _instance.spawnedTileIdsOrder[r]) = (_instance.spawnedTileIdsOrder[r], _instance.spawnedTileIdsOrder[i]);
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

        // TODO: animate from start transform to end
        collectible.transform.position = targetTransform.position;
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, targetTransform.position, targetTransform);
    }

    private int GetNextSpawnedId()
    {
        if (numSpawned > spawnedTileIdsOrder.Length - 1)
        {
            // Spawned everything!
            return -1;
        }
        int nextSpawn = spawnedTileIdsOrder[numSpawned];
        numSpawned += 1;
        return nextSpawn;
    }
}