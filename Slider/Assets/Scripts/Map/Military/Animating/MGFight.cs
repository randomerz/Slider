using UnityEngine;

public class MGFight : IMGAnimatable
{
    public MilitaryUnit unit;
    public MilitaryUnit unitOther;
    public STile stile;

    private const float FIGHT_DURATION = 3;

    public MGFight(MilitaryUnit unit, MilitaryUnit unitOther, STile stile) 
    {
        this.unit = unit;
        this.unitOther = unitOther;
        this.stile = stile;
    }

    public void Execute(System.Action finishedCallback)
    {
        Transform t;
        if (stile != null)
        {
            t = stile.transform;
        }
        else if (unitOther.AttachedSTile != null)
        {
            t = unitOther.AttachedSTile.transform;
        }
        else
        {
            t = unit.NPCController.transform;
        }

        unit.NPCController.FlashForDuration(FIGHT_DURATION);
        unitOther.NPCController.FlashForDuration(FIGHT_DURATION);
        MilitaryTurnAnimator.SpawnFightParticles(t);
        CoroutineUtils.ExecuteAfterDelay(
            () => finishedCallback?.Invoke(),
            unit,
            FIGHT_DURATION
        );
        
    }
}