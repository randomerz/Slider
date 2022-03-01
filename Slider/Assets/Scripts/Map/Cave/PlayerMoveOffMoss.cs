using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMoveOffMoss : MonoBehaviour
{
    public Tilemap mossMap;
    public Transform player;
    public Transform playerRespawn;

    public float playerBoopSpeed;

    //private bool movingPlayer;

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        }
        mossMap = GetComponent<Tilemap>();
    }

    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player.transform.position = playerRespawn.position;
        }
    }
    */

    /*
    public IEnumerator MovePlayerOffMoss()
    {
        movingPlayer = true;

        //Move player off the moss
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += playerBoopSpeed;
            player.transform.position = Vector3.Lerp(player.transform.position, playerRespawn.position, t);
            yield return new WaitForSeconds(0.1f);
        }

        // Debug.Log(player.transform.position);
        // Debug.Log(playerRespawn.position);
        movingPlayer = false;
    }
    */
    
    internal void CheckPlayerOnMoss(Vector3Int pos)
    {
        //L: Determine if the player is on the moss while it is growing
        Vector2Int mossTile = TileUtil.WorldToTileCoords(mossMap.CellToWorld(pos));
        bool movePlayerOffMoss = mossTile.Equals(TileUtil.WorldToTileCoords(player.transform.position));
        if (movePlayerOffMoss)
        {
            player.transform.position = playerRespawn.position;
        }
    }
    
}
