using UnityEngine;
using UnityEngine.Tilemaps;
// using static UnityEditor.Experimental.GraphView.GraphView;

public class MagitechMoveOffMoss : MonoBehaviour
{
    [SerializeField] private Transform playerRespawn;

    private Tilemap _mossColliders;
    private Player _player;


    private void Awake()
    {
       Init();
    }

    private void Init()
    {
        _mossColliders = GetComponent<Tilemap>();
        _player = FindObjectOfType<Player>();
    }

    private void Start()
    {
        CheckPlayerCollidingWithMoss();
    }

    public void CheckPlayerCollidingWithMoss()
    {
        if(_player == null)
            Init();
        Vector3Int playerCellCoords = _mossColliders.WorldToCell(_player.transform.position);
        foreach(Vector3Int pos in _mossColliders.cellBounds.allPositionsWithin)
        {
            if(_mossColliders.GetTile(pos) != null && playerCellCoords == pos)
                MovePlayerOffMoss();
        }
    }

    private void MovePlayerOffMoss()
    {
        _player.transform.position = playerRespawn.position;
        AudioManager.Play("Hurt");
    }
}
