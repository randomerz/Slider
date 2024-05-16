using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The very advanced strategy of telling each unit to run at the closest enemy they can kill and otherwise just move around randomly.
/// </summary>
public class MoveTowardsStrategy : ICommanderStrategy
{
    public void PerformTurn(List<MilitaryUnit> unitsToCommand)
    {
        unitsToCommand.ForEach(unit => PerformMoveForUnit(unit));
    }

    private void PerformMoveForUnit(MilitaryUnit unit)
    {
        MilitaryUnit closestKillableUnit = ClosestKillableUnitTo(unit);

        if (closestKillableUnit != null)
        {
            Vector2Int move = DirectionTowardsUnit(movingUnit: unit, target: closestKillableUnit);
            unit.GridPosition += move;
            Debug.Log($"Moving towards unit: {move}");
        }
        else
        {
            Vector2Int move = RandomDirection(unit);
            unit.GridPosition += move;
            Debug.Log($"Moving randomly: {move}");
        }
    }

    private MilitaryUnit ClosestKillableUnitTo(MilitaryUnit unit)
    {
        return MilitaryUnit.ActiveUnits.ToList()
                                       .Where(otherUnit => MilitaryCombat.WinnerOfBattleBetween(unit, otherUnit) == unit)
                                       .OrderBy(otherUnit => Vector2.Distance(unit.GridPosition, otherUnit.GridPosition))
                                       .FirstOrDefault();
    }

    private Vector2Int DirectionTowardsUnit(MilitaryUnit movingUnit, MilitaryUnit target)
    {
        int xDirection = Math.Sign(target.GridPosition.x - movingUnit.GridPosition.x);
        int yDirection = Math.Sign(target.GridPosition.y - movingUnit.GridPosition.y);

        // If we could move in either direction, pick one at random
        if (xDirection != 0 && yDirection != 0)
        {
            if (UnityEngine.Random.Range(0, 2) == 1)
            {
                xDirection = 0;
            }
            else
            {
                yDirection = 0;
            }
        }

        Vector2Int move = new(xDirection, yDirection);

        if (!PositionIsValid(movingUnit, movingUnit.GridPosition + move))
        {
            Vector2Int pos = movingUnit.GridPosition + move;
            Debug.Log($"Position Was Invalid: {pos.x}, {pos.y}");
            return RandomDirection(movingUnit);
        }

        return new Vector2Int(xDirection, yDirection);
    }

    private Vector2Int RandomDirection(MilitaryUnit unitToMove)
    {
        IEnumerable<Vector2Int> validDirections = DirectionUtil.Cardinal4.Where(direction => PositionIsValid(unitToMove, unitToMove.GridPosition + direction));
        if (validDirections.Count() == 0)
        {
            return new Vector2Int(0, 0);
        }

        int randomIndex = UnityEngine.Random.Range(0, validDirections.Count() - 1);
        return validDirections.ToArray()[randomIndex];
    }

    private bool PositionIsValid(MilitaryUnit unit, Vector2Int gridPosition)
    {
        return PositionIsInBounds(gridPosition)
            && !MilitaryCombat.GridPositionsOccupiedByAlliesOf(unit).Contains(gridPosition);
    }

    private bool PositionIsInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < SGrid.Current.Width
            && position.y >= 0 && position.y < SGrid.Current.Height;
    }
}
