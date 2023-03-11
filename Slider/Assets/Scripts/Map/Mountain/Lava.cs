using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Lava : MonoBehaviour
{
    public Tilemap tilemap;
    private List<Vector3> tileWorldLocations;
    //public GameObject LavaPS;
    public ParticleSystem ps;
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

        ps.maxParticles *= numtiles;
        ps.emissionRate *= numtiles;

        int numpaths = compositeCollider2D.pathCount;
        polygonCollider2D.pathCount = numpaths;
        for(int i = 0; i < numpaths; i++) {
            Vector2[] points = new Vector2[compositeCollider2D.GetPathPointCount(i)];
            compositeCollider2D.GetPath(i, points);
            polygonCollider2D.SetPath(i, points);
        }

       /* foreach(Vector3 loc in tileWorldLocations) {
            Instantiate(LavaPS, loc, Quaternion.identity, this.transform);
        }*/
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.GetComponent<Meltable>()){
            other.gameObject.GetComponent<Meltable>().AddLava();
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if(other.gameObject.GetComponent<Meltable>())
            other.gameObject.GetComponent<Meltable>().RemoveLava();
    }
}
