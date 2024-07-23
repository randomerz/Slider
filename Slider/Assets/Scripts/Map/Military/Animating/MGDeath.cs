using System.Collections.Generic;
using UnityEngine;

public class MGDeath : IMGAnimatable
{
    public MGDeath(MilitaryUnit unit) 
    {
        this.unit = unit;
    }

    public override void Execute(System.Action finishedCallback)
    {
        // Debug.Log($"Executed death for {unit.UnitTeam} {unit.UnitType}");
        unit.DoDeathAnimation(finishedCallback);
    }

    public override bool DoesOverlapWithAny(List<IMGAnimatable> otherAnimatable)
    {
        foreach (IMGAnimatable animatable in otherAnimatable)
        {
            if (MilitaryTurnAnimator.IsUnitInAnimatable(animatable, unit))
            {
                return true;
            }
        }
        return false;
    }
}