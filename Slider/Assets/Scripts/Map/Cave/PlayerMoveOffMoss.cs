using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMoveOffMoss : MonoBehaviour
{
    public Tilemap mossMap;
    public Transform player;
    public Transform playerRespawn;

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
