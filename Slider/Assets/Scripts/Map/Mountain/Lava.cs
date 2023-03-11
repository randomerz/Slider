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
