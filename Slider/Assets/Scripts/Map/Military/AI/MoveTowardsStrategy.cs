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
    private bool log = false;

    public void PerformTurn(List<MilitaryUnit> unitsToCommand)
    {
        if (log) Debug.Log($"===== Starting turn =====");
        unitsToCommand
                .OrderBy(unit => ClosestUnitDistance(unit))
                .ToList()
                .ForEach(unit => PerformMoveForUnit(unit));
        if (log) Debug.Log($"===== Ending turn =====");
    }

    private void PerformMoveForUnit(MilitaryUnit unit)
    {
        if (log) Debug.Log($"- Turn for {unit.UnitType} at {unit.GridPosition}. Distance {ClosestUnitDistance(unit)}");
        if (unit.UnitStatus != MilitaryUnit.Status.Active)
        {
            return;
        }

        // DC: this strategy should just move towards the closest unit! its dumb!
        // MilitaryUnit closestKillableUnit = ClosestKillableUnitTo(unit);
        MilitaryUnit closestUnit = ClosestUnitTo(unit);

        Vector2Int dir;
        if (closestUnit != null)
        {
            dir = DirectionTowardsUnit(movingUnit: unit, target: closestUnit);
            // Debug.Log($"Moving towards unit: {dir}");
        }
        else
        {
            dir = RandomDirection(unit);
            // Debug.Log($"Moving randomly: {dir}");
        }

        // Warning!! The main body of the enemy unit does not get updated but the npc part is animated
        Vector2Int newGridPos = unit.GridPosition + dir;
        MGMove move = new MGMove(unit, unit.GridPosition, newGridPos, null, null);
        MilitaryTurnAnimator.AddToQueue(move);
        unit.GridPosition = newGridPos; // Will call combat checks
    }

    private MilitaryUnit ClosestKillableUnitTo(MilitaryUnit unit)
    {
        return MilitaryUnit.ActiveUnits.ToList()
                                       .Where(otherUnit => otherUnit.UnitTeam != unit.UnitTeam)
                                       .Where(otherUnit => MilitaryCombat.WinnerOfBattleBetween(unit, otherUnit) == unit)
                                       .OrderBy(otherUnit => Vector2.Distance(unit.GridPosition, otherUnit.GridPosition))
                                       .FirstOrDefault();
    }

    private MilitaryUnit ClosestUnitTo(MilitaryUnit unit)
    {
        return MilitaryUnit.ActiveUnits.ToList()
                                       .Where(otherUnit => otherUnit.UnitTeam != unit.UnitTeam)
                                       .OrderBy(otherUnit => Vector2.Distance(unit.GridPosition, otherUnit.GridPosition))
                                       .FirstOrDefault();
    }

    private float ClosestUnitDistance(MilitaryUnit unit)
    {
        return MilitaryUnit.ActiveUnits.ToList()
                                       .Where(otherUnit => otherUnit.UnitTeam != unit.UnitTeam)
                                       .Select(otherUnit => Vector2.Distance(unit.GridPosition, otherUnit.GridPosition))
                                       .DefaultIfEmpty()
                                       .Min();
    }

    private Vector2Int DirectionTowardsUnit(MilitaryUnit movingUnit, MilitaryUnit target)
    {
        int xDirection = Math.Sign(target.GridPosition.x - movingUnit.GridPosition.x);
        int yDirection = Math.Sign(target.GridPosition.y - movingUnit.GridPosition.y);

        List<Vector2Int> directionPriorities;

        // If we could move in either direction, pick one at random
        if (xDirection != 0 && yDirection != 0)
        {
            if (SeededRandom.Range(0, 2) == 1)
            {
                directionPriorities = new() {
                    new(0, yDirection),
                    new(xDirection, 0),
                    new(0, -yDirection),
                    new(-xDirection, 0),
                };
            }
            else
            {
                directionPriorities = new() {
                    new(xDirection, 0),
                    new(0, yDirection),
                    new(-xDirection, 0),
                    new(0, -yDirection),
                };
            }
        }
        else // Otherwise they were strictly in one direction from us -- x or y == 0
        {
            directionPriorities = new() {
                new(xDirection, yDirection),
                new(yDirection, xDirection),
                new(-xDirection, -yDirection),
                new(-yDirection, -xDirection),
            };
        }

        foreach (Vector2Int direction in directionPriorities)
        {
            if (PositionIsValid(movingUnit, movingUnit.GridPosition + direction))
            {
                return direction;
            }
        }

        Debug.Log($"[MG] Unit at position {movingUnit.GridPosition.x}, {movingUnit.GridPosition.y} couldn't move anywhere.");
        return RandomDirection(movingUnit);
    }

    private Vector2Int RandomDirection(MilitaryUnit unitToMove)
    {
        IEnumerable<Vector2Int> validDirections = DirectionUtil.Cardinal4.Where(direction => PositionIsValid(unitToMove, unitToMove.GridPosition + direction));
        if (validDirections.Count() == 0)
        {
            return new Vector2Int(0, 0);
        }

        int randomIndex = SeededRandom.Range(0, validDirections.Count() - 1);
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
