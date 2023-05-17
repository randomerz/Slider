using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MGSimInitData
{
    public MGUnits.Job job;
    public MGUnits.Allegiance allegiance;
    public int min;
    public int max;
}

public class MGSimulator : MonoBehaviour
{
    [SerializeField] private List<MGSimInitData> initDataParams;

    private MGSpace[,] _board;
    private Vector2Int _boardDims;
    private MGEventSender _eventSender;

    private void Awake()
    {
        AudioManager.PlayMusic("MilitarySim");
        InitRandomUniform(new Vector2Int(4, 4));
        PrintSimulatorState();
    }

    public void InitEmpty(Vector2Int boardDims)
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

        _eventSender = new MGEventSender();

        //Initialize a supply tile
        //SpawnSupplyDrop(initSupplyDrop);

        //StartCoroutine(_eventSender.ProcessQueue());
    }

    public void InitRandomUniform(Vector2Int boardDims)
    {
        InitEmpty(boardDims);

        for (int x = 0; x < boardDims.x; x++)
        {
            for (int y = 0; y < boardDims.y; y++)
            {
                foreach (MGSimInitData unitData in initDataParams)
                {
                    int quantity = Random.Range(unitData.min, unitData.max);
                    _board[x, y].AddUnits(unitData.job, unitData.allegiance, quantity);
                }
            }
        }
    }

    private void PrintSimulatorState()
    {
        for (int x = 0; x < _boardDims.x; x++)
        {
            for (int y = 0; y < _boardDims.y; y++)
            {
                Debug.Log($"Printing board state for ({x}, {y})");
                _board[x, y].PrintUnitData();
            }
        }
    }

    //private void SpawnSupplyDrop(Vector2Int pos)
    //{
    //    Debug.Log($"Spawn Supplies at {pos}");
    //    MGSupply supply = new MGSupply();
    //    _board[pos.x, pos.y].AddEntity(supply);
    //    _eventSender.QueueEvent(new MGSpawnEvent(supply, pos));
    //}
}