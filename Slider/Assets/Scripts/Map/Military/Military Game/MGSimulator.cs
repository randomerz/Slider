using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGSimulator : MonoBehaviour
{
    [SerializeField] private List<MGUnitData.Data> possibleUnits;

    private Vector2Int _boardDims;
    private MGSpace[,] _board;
    private List<MGUnit> _units;
    //private MGEventSender _eventSender;

    public static event System.EventHandler AfterInit;

    public delegate void _OnUnit(MGUnit unit);
    public static event _OnUnit OnUnitSpawn;

    private void Awake()
    {
        //AudioManager.PlayMusic("MilitarySim");
        InitEmptyGrid(new Vector2Int(4, 4));
        AfterInit?.Invoke(this, null);
    }

    private void Start()
    {
        //_board[0, 3].SpawnSupplyDrop();
        PopulateRandom(_boardDims);
        PrintSimulatorState();
    }

    public MGSpace GetSpace(int x, int y)
    {
        if (x >= _boardDims.x || y >= _boardDims.y)
        {
            Debug.LogError("Space is out of bounds.");
            return null;
        }
        return _board[x, y];
    }

    public void InitEmptyGrid(Vector2Int boardDims)
    {
        //Clear the grid
        Debug.Log($"Created {boardDims.x} x {boardDims.y} Board");
        _boardDims = boardDims;
        _board = new MGSpace[boardDims.x, boardDims.y];
        for (int x = 0; x < boardDims.x; x++)
        {
            for (int y = 0; y < boardDims.y; y++)
            {
                _board[x, y] = new MGSpace(x, y);
            }
        }

        //_eventSender = new MGEventSender();

        //Initialize a supply tile
        //SpawnSupplyDrop(initSupplyDrop);

        //StartCoroutine(_eventSender.ProcessQueue());
    }

    public void PopulateRandom(Vector2Int boardDims)
    {
        if (possibleUnits.Count == 0)
        {
            Debug.LogError("No possible units. Cannot populate");
            return;
        }

        _units = new List<MGUnit>();

        for (int y = 0; y < boardDims.y; y++)
        {
            for (int x = 0; x < boardDims.x; x++)
            {
                if (Random.Range(0f, 1f) > 0.5f)
                {
                    int randUnitId = Random.Range(0, possibleUnits.Count);
                    MGUnitData.Data randUnitData = possibleUnits[randUnitId];

                    SpawnUnit(x, y, randUnitData);
                }

                //foreach (MGUnitData.Data unitData in possibleUnits)
                //{
                //    int quantity = Random.Range(0, 1);
                //    MGUnitData.Data tempUnit = new MGUnitData.Data(unitData.job, unitData.side);
                //    if (unitData.side == MGUnitData.Allegiance.Any)
                //    {
                //        if (Random.Range(0f, 1f) > 0.5f)
                //        {
                //            tempUnit.side = MGUnitData.Allegiance.Ally;
                //        } else
                //        {
                //            tempUnit.side = MGUnitData.Allegiance.Enemy;
                //        }
                //    }
                //    _board[x, y].AddUnits(tempUnit, quantity);
                //}
            }
        }
    }

    private void SpawnUnit(int x, int y, MGUnitData.Data data)
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

    private void PrintSimulatorState()
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