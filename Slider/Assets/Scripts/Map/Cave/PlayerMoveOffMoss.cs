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

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        }
        mossMap = GetComponent<Tilemap>();
    }

    private void OnEnable()
    {
        CaveMossManager.MossIsGrowing += CheckPlayerOnMoss;
    }

    private void OnDisable()
    {
        CaveMossManager.MossIsGrowing -= CheckPlayerOnMoss;
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

    internal void CheckPlayerOnMoss(object sender, CaveMossManager.MossIsGrowingArgs e)
    {
        //Each mossMap is tied to a specific respawn point.
        if (e.mossMap == mossMap)   //This check (hopefully) avoids bugs with teleporting to the wrong places (tile 4 bug)
        {
            //L: Determine if the player is on the moss while it is growing
            Vector2Int mossTile = TileUtil.WorldToTileCoords(e.mossMap.CellToWorld(e.cellPos));
            bool movePlayerOffMoss = mossTile.Equals(TileUtil.WorldToTileCoords(player.transform.position)) && e.isGrowing;
            if (movePlayerOffMoss)
            {
                player.transform.position = playerRespawn.position;
                AudioManager.Play("Hurt");
            }
        }
    }
    
}
