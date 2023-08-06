using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class MoveObjectsOffIce : MonoBehaviour
{
    public Transform playerRespawn;
    public LayerMask blocksSpawnMask;

    [Serializable]
    public class TileRespawn
    {
        public int islandid;
        public Transform respawn;
    }

    [SerializeField] private TileRespawn[] respawns;

    private Tilemap colliders;
    private Transform player;
    private List<Transform> otherObjects = new List<Transform>();
    private STile stile;

    private void Start() 
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        colliders = GetComponent<Tilemap>();
        stile = GetComponentInParent<STile>();
    }

    public void CheckPlayerOnIce()
    {
        TileBase tile = colliders.GetTile(colliders.WorldToCell(player.position));
        if(tile != null && colliders.ContainsTile(tile))
        {
            if(CheckTileBelow()) 
            {
                Vector3 checkpos = player.position + new Vector3(0, -100, 0);
                int tries = 0;
                do
                {
                    var cast = Physics2D.OverlapCircle(checkpos, 0.5f, blocksSpawnMask);
                    if(cast == null || cast.gameObject.GetComponent<STile>())
                    {
                        AudioManager.Play("Hurt");
                        player.position = checkpos;
                        return;
                    }
                    else {
                        checkpos += Vector3.right;
                        tries++;
                    }
                }
                while (tries < 5);
            }
            player.position = playerRespawn.position;
            AudioManager.Play("Hurt");
        }
    }

    public void CheckObjectsOnIce()
    {
        int objCount = 1;
        foreach(Transform t in otherObjects) {
            TileBase tile = colliders.GetTile(colliders.WorldToCell(t.position));
            if(tile != null && colliders.ContainsTile(tile)) 
            {
                Minecart mc = t.GetComponent<Minecart>();
                bool moved = false;
                if(CheckTileBelow()) 
                {
                    if(mc != null && mc.isMoving)
                    {
                        mc.Drop(SGrid.Current.GetGrid()[stile.x, stile.y - 2]);
                        //moved = mc.TryDropFromIce();
                        //print ("minecart");
                    }
                    else{
                    Vector3 checkpos = t.position + new Vector3(0, -100, 0);
                    int tries = 0;
                    do
                    {
                        var cast = Physics2D.OverlapCircle(checkpos, 0.5f, blocksSpawnMask);
                        if(cast == null || cast.gameObject.GetComponent<STile>())
                        {
                            t.position = checkpos;
                            moved = true;
                        }
                        else {
                            checkpos += Vector3.right;
                            tries++;
                        }
                    }
                    while (tries < 5 && !moved);
                    }
                }
                
               
                if(!moved) {
                     mc?.StopMoving();
                    t.position = playerRespawn.position + (Mathf.Min(objCount,3)) * Vector3.right;
                }
                objCount++;
            }
        }
        otherObjects.Clear();
    }


    private bool CheckTileBelow() => stile.y > 1 && SGrid.Current.GetGrid()[stile.x, stile.y - 2].isTileActive;

    private bool CheckCollidersBelow() => true;

    private Transform GetRespawnByIslandID(int islandid)
    {
        foreach(TileRespawn tr in respawns)
        {
            if(tr.islandid == islandid)
            {
                return(tr.respawn);
            }
        }
        Debug.LogWarning($"No respawn point found for tile {islandid}. Defaulting to upper-level respawn");
        return playerRespawn;
    }

    private Vector3 GetDropLocation(Transform t)
    {
        return t.position;
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
