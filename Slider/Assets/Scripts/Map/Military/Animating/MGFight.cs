using UnityEngine;

public class MGFight : IMGAnimatable
{
    public MilitaryUnit unit;
    public MilitaryUnit unitOther;

    private const float FIGHT_DURATION = 3;

    public MGFight(MilitaryUnit unit, MilitaryUnit unitOther) 
    {
        this.unit = unit;
        this.unitOther = unitOther;
    }
    
    public void Execute(System.Action finishedCallback)
    {
        Transform t;
        if (unit.AttachedSTile != null)
        {
            t = unit.AttachedSTile.transform;
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
        
        AudioManager.PickSound("UI Click").WithPitch(0.6f).AndPlay();
        for (float i = 0.5f; i < FIGHT_DURATION; i += 0.5f)
        {
            CoroutineUtils.ExecuteAfterDelay(
                () => AudioManager.PickSound("UI Click").WithPitch(0.5f).AndPlay(),
                unit,
                i
            );
        }
    }
}