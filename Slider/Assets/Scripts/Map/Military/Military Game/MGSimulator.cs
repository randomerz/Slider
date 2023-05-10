using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGSimulator : MonoBehaviour
{
    private MGSpace[,] _board;
    private Vector2Int _boardDims;
    private MGEventSender _eventSender;

    public void Init(Vector2Int boardDims, Vector2Int initSupplyDrop, MGEventSender eventSender)
    {
        //Clear the grid
        Debug.Log($"Created {boardDims.x} x {boardDims.y} Board");
        _boardDims = boardDims;
        _board = new MGSpace[boardDims.x, boardDims.y];
        for (int x = 0; x < boardDims.x; x++)
        {
            for (int y = 0; y < boardDims.y; y++)
            {
                _board[x, y] = new MGSpace();
            }
        }

        _eventSender = eventSender;

        //Initialize a supply tile
        SpawnSupplyDrop(initSupplyDrop);

        StartCoroutine(_eventSender.ProcessQueue());
    }

    private void SpawnSupplyDrop(Vector2Int pos)
    {
        Debug.Log($"Spawn Supplies at {pos}");
        MGSupply supply = new MGSupply();
        _board[pos.x, pos.y].AddEntity(supply);
        _eventSender.QueueEvent(new MGSpawnEvent(supply, pos));
    }
}