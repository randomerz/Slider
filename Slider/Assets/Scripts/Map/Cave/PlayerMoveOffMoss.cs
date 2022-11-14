using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMoveOffMoss : MonoBehaviour
{
    [SerializeField] private Transform playerRespawn;

    private Tilemap mossColliders;

    private void Awake()
    {
        mossColliders = GetComponent<Tilemap>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckPlayerCollidingWithMoss(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckPlayerCollidingWithMoss(collision);
    }

    private void CheckPlayerCollidingWithMoss(Collision2D collision)
    {
        Player player = collision.collider.GetComponent<Player>();
        if (player != null)
        {
            Vector3Int playerCellCoords = mossColliders.WorldToCell(player.transform.position);
            Tile.ColliderType colliderType = mossColliders.GetColliderType(playerCellCoords);
            if (colliderType == Tile.ColliderType.Grid)
            {
                MovePlayerOffMoss(player.transform);
            }
        }
    }

    private void MovePlayerOffMoss(Transform player)
    {
        player.position = playerRespawn.position;
        AudioManager.Play("Hurt");
    }
}
