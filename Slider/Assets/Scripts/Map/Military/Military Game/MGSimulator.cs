using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

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

    //Each space has a 50% chance of spawning a unit
    public void PopulateRandom(float unitSpawnRate, List<MGUnitData> possibleUnits)
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
                if (Random.Range(0f, 1f) < unitSpawnRate)
                {
                    int randUnitId = Random.Range(0, possibleUnits.Count);
                    MGUnitData randUnitData = possibleUnits[randUnitId];

                    SpawnUnit(x, y, randUnitData);
                }
            }
        }
    }

    public MGUnit SpawnUnit(int x, int y, MGUnitData data)
    {
        MGUnit newUnit = new MGUnit(data, _board[x, y]);
        _units.Add(newUnit);
        OnUnitSpawn?.Invoke(newUnit);
        return newUnit;
    }

    public void MoveUnit(MGUnit unit, int dx, int dy)
    {
        Vector2Int currPos = unit.CurrSpace.GetPosition();
        Vector2Int newPos = currPos + new Vector2Int(dx, dy);
        if (PosIsOnBoard(newPos))
        {
            int unitI = 0;
            while(unitI < _units.Count)
            {
                MGUnit otherUnit = _units[unitI];
                if (otherUnit.CurrSpace == _board[newPos.x, newPos.y])
                {
                    EvaluateBattle(unit, otherUnit);
                    break;
                }
                unitI++;
            }
            unit.Move(_board[newPos.x, newPos.y]);
        }
    }

    public void EvaluateBattle(MGUnit attacker, MGUnit defender)
    {
        int battleResult = MGUnitData.Dominates(attacker.Data.job, defender.Data.job);
        //If the result is 0, both units get destroyed.
        if (battleResult >= 0)
        {
            //Attacker wins
            defender.Destroy();
            _units.Remove(defender);
        }
        if (battleResult <= 0)
        {
            //Defender wins
            attacker.Destroy();
            _units.Remove(attacker);
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