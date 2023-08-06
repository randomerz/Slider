using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LavaParticles : MonoBehaviour
{
    public Tilemap tilemap;
    public GameObject particleTile;

    private void Start() {
        if(tilemap == null) return;

        List<Vector3Int> positions = new List<Vector3Int>();
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {   
            if (tilemap.HasTile(pos)) 
            {
                positions.Add(pos);
            }
        }
        foreach (var pos in positions)
        {   
            GameObject go = Instantiate(particleTile, this.transform);
            go.transform.localPosition = pos + new Vector3(0.5f, 0.5f, 0);
            LavaParticleTile tile = go.GetComponent<LavaParticleTile>();
            var adjPos = GetAdjPositions(pos);
            for(int i = 0; i < 4; i++)
            {
                var adj = adjPos[i];
                if(!positions.Contains(adj))
                    tile.EnableParticles(i);
                else
                     tile.DisableParticles(i);
            }
        }

    }

    private Vector3Int[] GetAdjPositions(Vector3Int position)
    {
        Vector3Int right = position + Vector3Int.right;
        Vector3Int up = position + Vector3Int.up;
        Vector3Int left = position + Vector3Int.left;
        Vector3Int down = position + Vector3Int.down;
        return new Vector3Int[]{right, up, left, down};
    }
}
