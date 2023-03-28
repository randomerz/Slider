using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MaterialBasedEmitter : MonoBehaviour
{
    [SerializeField]
    private MonoBehaviour locatableRef;

    [SerializeField]
    private MaterialSoundMapping mapping;

    private ISTileLocatable locatable;

    // Start is called before the first frame update
    void Start()
    {
        if (locatableRef is ISTileLocatable)
        {
            locatable = locatableRef as ISTileLocatable;
        } else
        {
            Debug.LogError($"{locatableRef.name} does not actually implement ISTileLocatable ");
        }
    }

    public void Step()
    {
        Tilemap map = locatable.currentMaterialTilemap;

        if (!map)
        {
            Debug.LogWarning("No material tilemap found");
            PlayStepType(mapping.fallback);
            return;
        }

        Vector3Int p = map.WorldToCell(locatableRef.transform.position);

        TileBase tile = map.GetTile(p);

        if (!tile)
        {
            Debug.LogWarning($"No material tile corresponding to { p }");
            PlayStepType(mapping.fallback);
            return;
        }

        if (tile is Tile)
        {
            PlayStepType(mapping.Mapping[tile as Tile]);
        } else
        {
            Debug.LogWarning($"Not a proper tile: {tile.name}");
        }
    }

    private void PlayStepType(FMODUnity.EventReference e)
    {
        FMODUnity.RuntimeManager.PlayOneShot(e, transform.position);
    }
}
