using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGSimulator
{

    private Vector2Int _boardDims;
    public Vector2Int BoardDims => _boardDims;
    private MGSpace[,] _board;
    private List<MGUnit> _units;
    public List<MGUnit> Units => _units;

    public static event System.EventHandler AfterInit;

    public delegate void _OnUnit(MGUnit unit);
    public static event _OnUnit OnUnitSpawn;

    public void Init(Vector2Int boardDims)
    {
        _boardDims = boardDims;
        _board = new MGSpace[boardDims.x, boardDims.y];
        for (int x = 0; x < boardDims.x; x++)
        {
            for (int y = 0; y < boardDims.y; y++)
            {
                _board[x, y] = new MGSpace(x, y);
            }
        }
        _units = new List<MGUnit>();
        AfterInit?.Invoke(this, null);
    }

    public MGSpace GetSpace(int x, int y)
    {
        if (!PosIsOnBoard(new Vector2Int(x, y)))
        {
            Debug.LogError("Space is out of bounds.");
            return null;
        }
        return _board[x, y];
    }

    public void PopulateRandom(List<MGUnitData.Data> possibleUnits)
    {
        if (possibleUnits.Count == 0)
        {
            Debug.LogError("No possible units. Cannot populate");
            return;
        }

        _units = new List<MGUnit>();

        for (int y = 0; y < _boardDims.y; y++)
        {
            for (int x = 0; x < _boardDims.x; x++)
            {
                if (Random.Range(0f, 1f) > 0.5f)
                {
                    int randUnitId = Random.Range(0, possibleUnits.Count);
                    MGUnitData.Data randUnitData = possibleUnits[randUnitId];

                    SpawnUnit(x, y, randUnitData);
                }
            }
        }
    }

    public void SpawnUnit(int x, int y, MGUnitData.Data data)
    {
        MGUnit newUnit = new MGUnit(data, _board[x, y]);
        _units.Add(newUnit);
        OnUnitSpawn?.Invoke(newUnit);
    }

    public void MoveUnit(MGUnit unit, int dx, int dy)
    {
        Vector2Int currPos = unit.CurrSpace.GetPosition();
        Vector2Int newPos = currPos + new Vector2Int(dx, dy);
        if (PosIsOnBoard(newPos))
        {
            unit.Move(_board[newPos.x, newPos.y]);
        }
    }

    private bool PosIsOnBoard(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < _boardDims.x && pos.y < _boardDims.y;
    }

    public void PrintSimulatorState()
    {
        for (int y = 0; y < _boardDims.y; y++)
        {
            for (int x = 0; x < _boardDims.x; x++)
            {
                string boardStateDesc = $"Board state for ({x}, {y}): \n";

                foreach (MGUnit unit in _units)
                {
                    if (unit.CurrSpace == _board[x, y])
                    {
                        boardStateDesc += unit.GetUnitDescriptor();
                    }
                }

                Debug.Log(boardStateDesc);
            }
        }
    }
}