using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;
public class MGSimulator
{
    public enum MoveType
    {
        INVALID,
        WALK,   //Unit moves to an empty square
        BATTLE  //Unit moves onto an occupied enemy square
    }

    [System.Serializable]
    public struct UnitSpawnData
    {
        public int x, y;
        public MGUnitData data;
    }

    public struct UnitMoveData
    {
        public MGUnit movingUnit;
        public MoveType type;
        public Vector2Int newPos;
        public MGUnit occupyingUnit;   //Occupying unit, if exists (could be null)

        public UnitMoveData(MGUnit movingUnit, MoveType result, Vector2Int newPos, MGUnit occupyingUnit)
        {
            this.movingUnit = movingUnit;
            this.type = result;
            this.newPos = newPos;
            this.occupyingUnit = occupyingUnit;
        }
    }

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

    public void Reset()
    {
        foreach (MGUnit unit in _units)
        {
            unit.Destroy();
        }
        _units.Clear();
    }

    public void Populate(List<UnitSpawnData> spawnData)
    {
        foreach (UnitSpawnData spawn in spawnData)
        {
            SpawnUnit(spawn.x, spawn.y, spawn.data);
        }
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

    public MGUnit GetUnit(int x, int y)
    {
        for (int unitI = 0; unitI < _units.Count; unitI++)
        {
            MGUnit unit = _units[unitI];
            if (unit.CurrSpace == _board[x, y])
            {
                return unit;
            }
        }

        return null;
    }

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
        UnitMoveData move = GetMove(unit, dx, dy);
        if (move.type != MoveType.INVALID)
        {
            DoMove(move);
        }
    }

    public UnitMoveData GetMove(MGUnit unit, int dx, int dy)
    {
        Vector2Int currPos = unit.CurrSpace.GetPosition();
        Vector2Int newPos = currPos + new Vector2Int(dx, dy);

        if (!PosIsOnBoard(newPos))
        {
            return new UnitMoveData(unit, MoveType.INVALID, newPos, null);
        }

        MGUnit occupyingUnit = GetUnit(newPos.x, newPos.y);
        if (occupyingUnit != null)
        {
            if (unit.Data.side == occupyingUnit.Data.side)
            {
                //Can't move onto another allied piece.
                return new UnitMoveData(unit, MoveType.INVALID, newPos, occupyingUnit);
            }
            else
            {
                return new UnitMoveData(unit, MoveType.BATTLE, newPos, occupyingUnit);
            }
        }

        return new UnitMoveData(unit, MoveType.WALK, newPos, null);
    }

    public void DoMove(UnitMoveData move)
    {
        switch(move.type)
        {
            case MoveType.WALK:
                move.movingUnit.Move(_board[move.newPos.x, move.newPos.y]);
                break;
            case MoveType.BATTLE:
                EvaluateBattle(move.movingUnit, move.occupyingUnit);
                move.movingUnit.Move(_board[move.newPos.x, move.newPos.y]);
                break;
            case MoveType.INVALID:
            default:
                Debug.LogError("Attempted to perform invalid move.");
                break;
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