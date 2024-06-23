using UnityEngine;

public class MGFight : IMGAnimatable
{
    public static int numberOfActiveFights = 0;

    public MilitaryUnit unitOther;
    private System.Action onExecute;

    public static float FightDuration => MilitaryTurnAnimator.CurrentGlobalAnimationsSpeed switch 
    {
        MilitaryTurnAnimator.Speed.Fast => 1f,
        MilitaryTurnAnimator.Speed.Medium => 1.5f,
        MilitaryTurnAnimator.Speed.Slow => 3f,
        _ => 3f
    };

    public MGFight(MilitaryUnit unit, MilitaryUnit unitOther) 
    {
        this.unit = unit;
        this.unitOther = unitOther;

        AddUITracker();
        
        numberOfActiveFights += 1;

        CoroutineUtils.ExecuteAfterEndOfFrame(() => AttachDeadAliens(), unit);
    }

    private void AttachDeadAliens()
    {
        if (
            unit.UnitTeam == MilitaryUnit.Team.Alien &&
            unit.UnitStatus == MilitaryUnit.Status.Dead &&
            unitOther.UnitTeam == MilitaryUnit.Team.Player && 
            unitOther.AttachedSTile != null)
        {
            onExecute = () => {
                unit.AttachedSTile = unitOther.AttachedSTile;
                unit.GridPosition = unitOther.GridPosition;

                if (unit.transform.position != unitOther.transform.position)
                {
                    unit.NPCController.SetPosition(unitOther.transform.position);
                }
            };
        }
        else if (
            unitOther.UnitTeam == MilitaryUnit.Team.Alien &&
            unitOther.UnitStatus == MilitaryUnit.Status.Dead &&
            unit.UnitTeam == MilitaryUnit.Team.Player && 
            unit.AttachedSTile != null)
        {
            onExecute = () => {
                unitOther.AttachedSTile = unit.AttachedSTile;
                unitOther.GridPosition = unit.GridPosition;
                
                if (unit.transform.position != unitOther.transform.position)
                {
                    unitOther.NPCController.SetPosition(unit.transform.position);
                }
            };
        }
    }

    private void AddUITracker()
    {
        GameObject go = new GameObject("Fight Tracker Target");
        if (unit.AttachedSTile != null)
        {
            go.transform.SetParent(unit.AttachedSTile.transform);
            go.transform.position = MilitaryUnit.GridPositionToWorldPosition(unit.GridPosition);
        }
        else if (unitOther.AttachedSTile != null)
        {
            go.transform.SetParent(unitOther.AttachedSTile.transform);
            go.transform.position = MilitaryUnit.GridPositionToWorldPosition(unitOther.GridPosition);
        }
        else
        {
            Debug.LogError($"Nothing to attach tracker to!");
        }

        MilitaryUITrackerManager.AddFightTracker(go);

        unit.OnDeath.AddListener(() => {
            MilitaryUITrackerManager.RemoveTracker(go);
            GameObject.Destroy(go);
        });
        unitOther.OnDeath.AddListener(() => {
            MilitaryUITrackerManager.RemoveTracker(go);
            GameObject.Destroy(go);
        });
    }
    
    public override void Execute(System.Action finishedCallback)
    {
        onExecute?.Invoke();

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

        unit.NPCController.FlashForDuration(FightDuration);
        unitOther.NPCController.FlashForDuration(FightDuration);
        MilitaryTurnAnimator.SpawnFightParticles(t);
        CoroutineUtils.ExecuteAfterDelay(
            () => {
                numberOfActiveFights -= 1;
                finishedCallback?.Invoke();
            },
            unit,
            FightDuration
        );
        
        AudioManager.PickSound("UI Click").WithPitch(0.6f).AndPlay();
        for (float i = 0.5f; i < FightDuration; i += 0.5f)
        {
            CoroutineUtils.ExecuteAfterDelay(
                () => AudioManager.PickSound("UI Click").WithPitch(0.5f).AndPlay(),
                unit,
                i
            );
        }
    }
}