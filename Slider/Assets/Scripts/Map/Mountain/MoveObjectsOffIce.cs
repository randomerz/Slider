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
    private List<Transform> otherObjects = new List<Transform>();
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
        if(tile != null && colliders.ContainsTile(tile) && SGrid.GetSTileUnderneath(checkpos) != null)
        {
            if(!ItemPlacerSolver.TryPlaceItem(checkpos, player.transform, 10, blocksSpawnMask, true))
            {
                player.position = playerRespawn.position;
            }
            AudioManager.Play("Hurt");
        }
    }

    public void CheckObjectsOnIce()
    {
        int objCount = 1;
        foreach(Transform t in otherObjects) {
            TileBase tile = colliders.GetTile(colliders.WorldToCell(t.position));
            Vector3 checkpos = t.position + new Vector3(0, -100, 0);
            if(tile != null && colliders.ContainsTile(tile) && SGrid.GetSTileUnderneath(checkpos) != null) 
            {
                bool moved = false;
                Minecart mc = t.gameObject.GetComponent<Minecart>();
                if(mc != null && mc.isMoving)
                {
                    moved = mc.TryDrop(true);
                }
                if(!moved)
                {
                    if(!ItemPlacerSolver.TryPlaceItem(checkpos, t, 10, blocksSpawnMask, true))
                    {
                        mc?.StopMoving();
                        t.position = playerRespawn.position + objCount * Vector3.right;
                    }
                }
                objCount++;
            }
        }
        otherObjects.Clear();
    }
    private bool CheckTileBelow() => stile.y > 1 && SGrid.Current.GetGrid()[stile.x, stile.y - 2].isTileActive;

    private void OnTriggerEnter2D(Collider2D other) {
        Transform t = other.transform;
        if(t != player && (t.GetComponent<Item>())){
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
