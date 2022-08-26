using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MoveObjectsOffIce : MonoBehaviour
{
    public Transform playerRespawn;
    private Tilemap colliders;
    private Transform player;
    private List<Transform> otherObjects = new List<Transform>();

    private void Start() 
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        colliders = GetComponent<Tilemap>();
    }

    public void CheckPlayerOnIce()
    {
        TileBase tile = colliders.GetTile(colliders.WorldToCell(player.position));
        if(tile != null && colliders.ContainsTile(tile))
        {
            player.position = playerRespawn.position;
            AudioManager.Play("Hurt");
        }
    }

    public void CheckObjectsOnIce()
    {
        int objCount = 1;
        foreach(Transform t in otherObjects) {
            TileBase tile = colliders.GetTile(colliders.WorldToCell(player.position));
            if(tile != null && colliders.ContainsTile(tile)) 
            {
                Minecart mc = t.GetComponent<Minecart>();
                mc?.StopMoving();
                t.position = playerRespawn.position + (Mathf.Min(objCount,3)) * Vector3.right;
                objCount++;
            }
        }
        otherObjects.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Transform t = other.transform;
        if(t != player && (t.GetComponent<Anchor>() || t.GetComponent<Minecart>())){
            otherObjects.Add(t);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform t = other.transform;
        if(t != player) {
            otherObjects.Remove(t);
        }
    }
}
