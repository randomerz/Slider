using System.Collections.Generic;
using UnityEngine;

public class LavaParticleEdge : MonoBehaviour
{
    private List<Collider2D> collider2Ds = new List<Collider2D>();
    [SerializeField] private ParticleSystem ps;
    
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.GetComponent<LavaParticleTile>())
        {
            collider2Ds.Add(other);
            ps.Stop();
            ps.Clear();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        collider2Ds.Remove(other);
        if(collider2Ds.Count == 0)
            ps.Play();
    }
}
