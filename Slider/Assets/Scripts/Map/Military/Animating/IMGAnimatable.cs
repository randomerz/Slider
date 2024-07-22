

using System.Collections.Generic;

public abstract class IMGAnimatable
{
    public MilitaryUnit unit;

    public abstract void Execute(System.Action finishedCallback);
    public abstract bool DoesOverlapWithAny(List<IMGAnimatable> otherAnimatable);
}