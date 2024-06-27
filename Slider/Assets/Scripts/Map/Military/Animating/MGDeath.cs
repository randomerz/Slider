using UnityEngine;

public class MGDeath : IMGAnimatable
{
    public MGDeath(MilitaryUnit unit) 
    {
        this.unit = unit;
    }

    public override void Execute(System.Action finishedCallback)
    {
        unit.DoDeathAnimation(finishedCallback);
    }
}