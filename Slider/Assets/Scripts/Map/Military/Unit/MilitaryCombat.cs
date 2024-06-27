using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MilitaryUnit;

public class MilitaryCombat
{
    public static MilitaryUnit WinnerOfBattleBetween(MilitaryUnit unitA, MilitaryUnit unitB)
    {
        // On a tie, both units die
        if (unitA.UnitType == unitB.UnitType)
        {
            return null;
        }

        switch (unitA.UnitType)
        {
            case Type.Rock:
                return unitB.UnitType == Type.Scissors ? unitA : unitB;
            case Type.Paper:
                return unitB.UnitType == Type.Rock ? unitA : unitB;
            case Type.Scissors:
                return unitB.UnitType == Type.Paper ? unitA : unitB;
            default:
                return null;
        }
    }

    public static void ResolveBattle(MilitaryUnit unitA, MilitaryUnit unitB)
    {
        // Debug.Log($"Resolving Battle: '{unitA.gameObject.name}' vs '{unitB.gameObject.name}'");
        MilitaryUnit winner = WinnerOfBattleBetween(unitA, unitB);
        if (unitA != winner)
        {
            unitA.KillUnit();
        }
        if (unitB != winner)
        {
            unitB.KillUnit();
        }
    }

    public static List<Vector2Int> GridPositionsOccupiedByAlliesOf(MilitaryUnit unit)
    {
        IEnumerable<Vector2Int> occupiedGridPositions = ActiveUnits.ToList()
                                                                   .Where(otherUnit => otherUnit.UnitTeam == unit.UnitTeam)
                                                                   .Where(otherUnit => otherUnit.UnitStatus == Status.Active)
                                                                   .Select(otherUnit => otherUnit.GridPosition);
        return occupiedGridPositions.ToList();
    }
}
