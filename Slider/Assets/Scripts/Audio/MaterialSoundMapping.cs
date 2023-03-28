using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable Objects/Material Sound Mapping")]
public class MaterialSoundMapping : ScriptableObject
{
    [SerializeField]
    private MaterialSoundMappingEntry[] mappingList;
    public Dictionary<Tile, FMODUnity.EventReference> Mapping
    {
        get
        {
            if (_CachedMapping == null)
            {
                _CachedMapping = new Dictionary<Tile, FMODUnity.EventReference>();
                foreach (var m in mappingList)
                {
                    if (Mapping.ContainsKey(m.materialTile))
                    {
                        Debug.LogWarning($"{m.materialTile.name} is duplicated in preset {name}");
                    }
                    else
                    {
                        Mapping.Add(m.materialTile, m.eventRef);
                    }
                }
            } 
            return _CachedMapping;
        }
    }
    private Dictionary<Tile, FMODUnity.EventReference> _CachedMapping;

    [SerializeField]
    private FMODUnity.EventReference fallbackMapping;

    public FMODUnity.EventReference fallback => fallbackMapping;

    [System.Serializable]
    public struct MaterialSoundMappingEntry
    {
        public Tile materialTile;
        public FMODUnity.EventReference eventRef;
    }
}
