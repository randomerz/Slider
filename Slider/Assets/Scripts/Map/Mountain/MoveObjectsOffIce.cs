using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class MoveObjectsOffIce : MonoBehaviour
{
    public Transform playerRespawn;
    public LayerMask blocksSpawnMask;
    public Minecart minecart;

    private Tilemap colliders;
    private Transform player;
    private List<GameObject> otherObjects = new List<GameObject>();
    private STile stile;

    private void Start() 
    {
        player = Player.GetInstance().transform;
        colliders = GetComponent<Tilemap>();
        stile = GetComponentInParent<STile>();
    }

    public void CheckPlayerOnIce()
    {
        if(player == null) return;
        TileBase tile = colliders.GetTile(colliders.WorldToCell(player.position));
        Vector3 checkpos = player.position + new Vector3(0, -100, 0);
        if(tile != null && colliders.ContainsTile(tile))
        {
            if(SGrid.GetSTileUnderneath(checkpos) != null)
            {
                if(!ItemPlacerSolver.TryPlaceItem(checkpos, player, 10, blocksSpawnMask, true))
                {
                    player.position = playerRespawn.position;
                }
                AudioManager.Play("Hurt");
            }
            else
            {    
                player.position = playerRespawn.position;
                AudioManager.Play("Hurt");
                return;
            }
           
        }
    }

    public void CheckObjectsOnIce()
    {
        int objCount = 1;
        foreach(GameObject go in otherObjects) {
            Transform t = go.transform;
            TileBase tile = colliders.GetTile(colliders.WorldToCell(t.position));
            Vector3 checkpos = t.position + new Vector3(0, -100, 0);
            if(tile != null && colliders.ContainsTile(tile)) 
            {
                if(SGrid.GetSTileUnderneath(checkpos) != null)
                {
                    bool moved = false;
                    Minecart mc = t.gameObject.GetComponent<Minecart>();
                    if(mc != null)
                    { 
                        if(mc.isMoving)
                            moved = mc.TryDrop(true);
                        else if(mc.isOnTrack && SGrid.GetSTileUnderneath(checkpos).islandId == 1)
                            SaveSystem.Current.SetBool("MountainDidIcePatchTech", true);

                    }
                    if(!moved)
                    {
                        if(ItemPlacerSolver.TryPlaceItem(checkpos, t, 10, blocksSpawnMask, true))
                        {   
                            Anchor a;
                            if(go.TryGetComponent<Anchor>(out a))
                            {
                                a.DropThroughIce();
                            }
                        }
                        else
                        {
                            mc?.StopMoving();
                            t.position = playerRespawn.position + objCount * Vector3.right;
                        }
                    }
                }
                else
                {
                    t.position = playerRespawn.position + objCount * Vector3.right;
                }
                objCount++;
            }
        }
        otherObjects.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        GameObject go = other.gameObject;
        if(go != player.transform && go.GetComponent<Item>()){
            otherObjects.Add(go);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject go = other.gameObject;
        if(go.transform != player) {
            otherObjects.Remove(go);
        }
    }
}
