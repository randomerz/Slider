using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LavaParticles : MonoBehaviour
{
    public Tilemap tilemap;
    private List<Vector3> tileWorldLocations;
    public List<ParticleSystem> particleSystems;
    public CompositeCollider2D compositeCollider2D;
    public MeshFilter meshFilter;
    public PolygonCollider2D polygonCollider2D;
    private void Start() {
        if(tilemap == null) return;

        int numtiles = 0;
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {   
            if (tilemap.HasTile(pos)) 
                numtiles++;
        }

        Mesh mesh = compositeCollider2D.CreateMesh(false, false);
        meshFilter.mesh = mesh;

        //C: scale particles by mesh size
        foreach(ParticleSystem ps in particleSystems)
        {
            ps.maxParticles *= numtiles;
            ps.emissionRate *= numtiles;
        }

        int numpaths = compositeCollider2D.pathCount;
        polygonCollider2D.pathCount = numpaths;
        for(int i = 0; i < numpaths; i++) {
            Vector2[] points = new Vector2[compositeCollider2D.GetPathPointCount(i)];
            compositeCollider2D.GetPath(i, points);
            polygonCollider2D.SetPath(i, points);
        }
    }
}
