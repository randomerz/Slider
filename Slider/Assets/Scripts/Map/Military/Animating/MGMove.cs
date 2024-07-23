using System.Collections.Generic;
using UnityEngine;

public class MGMove : IMGAnimatable
{
    public Vector2Int startCoords;
    public Vector2Int endCoords;
    public STile startStile;
    public STile endStile;

    public MGMove(
        MilitaryUnit unit, 
        Vector2Int startCoords, 
        Vector2Int endCoords, 
        STile startStile, 
        STile endStile
    ) {
        this.unit = unit;
        this.startCoords = startCoords;
        this.endCoords = endCoords;
        this.startStile = startStile;
        this.endStile = endStile;
    }

    public override void Execute(System.Action finishedCallback)
    {
        unit.NPCController.AnimateMove(this, false, finishedCallback);
    }

    public override bool DoesOverlapWithAny(List<IMGAnimatable> otherAnimatable)
    {
        foreach (IMGAnimatable animatable in otherAnimatable)
        {
            if (animatable is MGDeath)
            {
                continue;
            }

            else if (animatable is MGFight fight)
            {
                if (MilitaryTurnAnimator.IsUnitInAnimatable(animatable, unit))
                {
                    return true;
                }

                if (
                    fight.unit.GridPosition == startCoords ||
                    fight.unit.GridPosition == endCoords ||
                    fight.unitOther.GridPosition == startCoords ||
                    fight.unitOther.GridPosition == endCoords
                ) {
                    return true;
                }
            }

            else if (animatable is MGMove move)
            {
                if (MilitaryTurnAnimator.IsUnitInAnimatable(animatable, unit))
                {
                    return true;
                }

                if (
                    move.startCoords == startCoords ||
                    // move.startCoords == endCoords ||
                    // move.endCoords == startCoords ||
                    move.endCoords == endCoords
                ) {
                    return true;
                }
            }
        }
        return false;
    }
}