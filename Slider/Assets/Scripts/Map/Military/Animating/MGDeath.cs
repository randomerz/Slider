using UnityEngine;

public class MGDeath : IMGAnimatable
{
    public MilitaryUnit unit;

    public MGDeath(MilitaryUnit unit) 
    {
        this.unit = unit;
    }

    public void Execute(System.Action finishedCallback)
    {
        unit.DoDeathAnimation(finishedCallback);
    }
}