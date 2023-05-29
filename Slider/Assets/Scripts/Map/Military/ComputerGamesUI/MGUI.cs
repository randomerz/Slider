using System.Collections.Generic;
using UnityEngine;
using static MGSimulator;

public class MGUI : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private MGUISquare[] squares;
    [SerializeField] private GameObject trackerPrefab;

    [SerializeField] private MilitarySimContext _simCtx;

    private MGUISquare selectedSquare;
    private Dictionary<MGUISquare, UnitMoveData> unitMoveOptions;

    public MGSimulator Sim => _simCtx.Simulator;

    public MGUISquare[] Squares => squares;

    private void Start()
    {
        selectedSquare = null;
        MGSimulator.OnUnitSpawn += OnUnitSpawn;
    }

    private void OnDisable()
    {
        MGSimulator.OnUnitSpawn -= OnUnitSpawn;
    }

    public MGUISquare GetSquare(Vector2Int pos)
    {
        int index = pos.y * width + pos.x;
        if (index < 0 || index >= squares.Length)
        {
            return null;
        }

        return squares[index];
    }

    public MGUISquare GetSquare(MGSpace space)
    {
        return GetSquare(space.GetPosition());
    }

    public MGUISquare GetSquare(int x, int y)
    {
        return GetSquare(new Vector2Int(x, y));
    }

    public void SelectSquare(MGUISquare square)
    {

        if (selectedSquare == null)
        {
            Vector2Int pos = square.GetPosition();
            MGUnit unit = Sim.GetUnit(pos.x, pos.y);
            selectedSquare = square;
            unitMoveOptions = new Dictionary<MGUISquare, UnitMoveData>();

            foreach (Vector2Int dir in DirectionUtil.Cardinal4)
            { 
                Vector2Int newPos = pos + dir;
                UnitMoveData move = Sim.GetMove(unit, dir.x, dir.y);
                MGUISquare moveSquare = GetSquare(newPos.x, newPos.y);
                if (moveSquare != null)
                {
                    switch (move.type)
                    {
                        case MoveType.WALK:
                            moveSquare.ChangeAnimState(MGUISquare.AnimStates.MOVE);
                            unitMoveOptions.Add(moveSquare, move);
                            break;
                        case MoveType.BATTLE:
                            moveSquare.ChangeAnimState(MGUISquare.AnimStates.BATTLE);
                            unitMoveOptions.Add(moveSquare, move);
                            break;
                        case MoveType.INVALID:
                            moveSquare.ChangeAnimState(MGUISquare.AnimStates.EMPTY);
                            break;
                    }

                }
            }

        }
    }

    private void OnUnitSpawn(MGUnit unit)
    {
        CreateTracker(unit);
    }

    private void CreateTracker(MGUnit unit)
    {
        GameObject trackerGO = GameObject.Instantiate(trackerPrefab, this.transform);
        MGUIUnitTracker tracker = trackerGO.GetComponent<MGUIUnitTracker>();
        tracker.Init(unit);
    }

    //private void DestroyTracker(MGUnitData.Data unit)
    //{
    //    MGUIUnitTracker tracker = _unitTrackers[unit];
    //    _unitTrackers.Remove(unit);
    //    Destroy(tracker.gameObject);
    //}
}
