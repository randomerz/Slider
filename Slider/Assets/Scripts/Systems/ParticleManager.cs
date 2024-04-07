using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParticleType
{
    SmokePoof,
    MiniSparkle,
}

public class ParticleManager : Singleton<ParticleManager>
{
    [System.Serializable]
    public struct ParticleTypePrefabPair
    {
        public ParticleType particleType;
        public GameObject prefab;
    }

    private static Dictionary<ParticleType, GameObject> particleTypeToPrefab = new Dictionary<ParticleType, GameObject>();
    [SerializeField] private List<ParticleTypePrefabPair> particleDefinitions = new List<ParticleTypePrefabPair>();

    public static ParticleManager Instance => _instance;
    
    void Awake()
    {
        if (particleTypeToPrefab == null || particleTypeToPrefab.Count == 0)
            Init();
    }
    
    private void Init()
    {
        particleTypeToPrefab = new Dictionary<ParticleType, GameObject>();

        foreach (ParticleTypePrefabPair pair in particleDefinitions)
        {
            particleTypeToPrefab[pair.particleType] = pair.prefab;
        }
    }

    public static void SpawnParticle(ParticleType type, Vector3 position, Transform parent=null)
    {
        SpawnParticle(type, position, Quaternion.identity, parent);
    }

    public static void SpawnParticle(ParticleType type, Vector3 position, Quaternion rotation, Transform parent=null)
    {
        GameObject prefab = GetPrefab(type);

        GameObject.Instantiate(prefab, position, rotation, parent);
    }

    public static GameObject GetPrefab(ParticleType type)
    {
        if (particleTypeToPrefab == null || particleTypeToPrefab.Count == 0)
            _instance.Init();
        
        if (!particleTypeToPrefab.ContainsKey(type))
        {
            Debug.LogError(type + " was not in the particle definitions!");
            return null;
        }
        
        return particleTypeToPrefab[type];
    }
}
