using UnityEngine;
using UnityEngine.Tilemaps;
// using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMoveOffMoss : MonoBehaviour
{
    [SerializeField] private Transform playerRespawn;

    private Tilemap _mossColliders;
    private Player _player;


    private void Awake()
    {
        _mossColliders = GetComponent<Tilemap>();
        _player = FindObjectOfType<Player>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<Player>() != null)
        {
            CheckPlayerCollidingWithMoss();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<Player>() != null)
        {
            CheckPlayerCollidingWithMoss();
        }
    }

    public void CheckPlayerCollidingWithMoss()
    {
        Vector3Int playerCellCoords = _mossColliders.WorldToCell(_player.transform.position);
        if (_mossColliders.cellBounds.Contains(playerCellCoords))
        {
            Tile.ColliderType colliderType = _mossColliders.GetColliderType(playerCellCoords);
            if (colliderType == Tile.ColliderType.Grid)
            {
                MovePlayerOffMoss();
            }
        }
    }

    private void MovePlayerOffMoss()
    {
        _player.transform.position = playerRespawn.position;
        AudioManager.Play("Hurt");
    }
}
