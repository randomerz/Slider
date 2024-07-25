using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// Assumptions
/// - Units can only mgmove once per turn

public class MilitaryTurnAnimator : Singleton<MilitaryTurnAnimator>
{
    public static System.EventHandler<System.EventArgs> AfterCheckQueue;

    public enum QueueStatus 
    {
        Off,
        Ready, // Might still be processing but send the next
        Processing, // Proccessing animations
    }
    public enum Speed 
    {
        Slow,
        Medium,
        Fast,
    }

    public static Speed CurrentGlobalAnimationsSpeed = Speed.Slow;
    public static Speed BaseGlobalAnimationsSpeed = Speed.Slow;

    private MGTurnAnimatables endOfQueueTurn = null; // This is what is being added to -- not the one currently being processesd
    private Queue<MGTurnAnimatables> turnQueue = new();

    private List<IMGAnimatable> activeMoves = new();
    private QueueStatus status = QueueStatus.Off;

    private float timeLastMoveExecuted;
    private IMGAnimatable lastMoveExecuted;
    
    [SerializeField] private GameObject fightParticles;

    private void Awake()
    {
        InitializeSingleton();
        CloseAndAddNewTurnToQueue();
        CurrentGlobalAnimationsSpeed = Speed.Slow;
        BaseGlobalAnimationsSpeed = Speed.Slow;
    }

    private void OnEnable()
    {
        MilitaryTurnManager.OnEnemyEndTurn += OnEnemyEndTurn;
    }

    private void OnDisable()
    {
        MilitaryTurnManager.OnEnemyEndTurn -= OnEnemyEndTurn;
    }

    // Usually it will be player moves -> enemy moves, but stuff can happen outside that (like spawning a unit under an enemy)
    public static void AddToQueue(IMGAnimatable move) => _instance._AddToQueue(move);

    public void _AddToQueue(IMGAnimatable move)
    {
        // Debug.Log($"Added {move} to queue for {move.unit.UnitTeam} {move.unit.UnitType}");
        endOfQueueTurn.mgAnimatables.Add(move);
        
        // Speed up animations if we're adding and there is already another turn
        if (turnQueue.Count > 1)
        {
            SpeedUpAnimations();
        }
        
        CheckQueue();
    }

    private void OnEnemyEndTurn(object sender, System.EventArgs e) => CloseAndAddNewTurnToQueue();

    public void CloseAndAddNewTurnToQueue()
    {
        endOfQueueTurn = new();
        turnQueue.Enqueue(endOfQueueTurn);
    }

    public static bool IsUnitInActiveOrQueue(MilitaryUnit unit) => _instance._IsUnitInActiveOrQueue(unit);

    public bool _IsUnitInActiveOrQueue(MilitaryUnit unit)
    {
        // Active
        foreach (IMGAnimatable i in activeMoves)
        {
            if (IsUnitInAnimatable(i, unit))
                return true;
        }

        // Queue
        foreach (MGTurnAnimatables turn in turnQueue)
        {
            foreach (IMGAnimatable i in turn.mgAnimatables)
            {
                if (IsUnitInAnimatable(i, unit))
                    return true;
            }
        }

        return false;
    }

    public static bool IsUnitInAnimatable(IMGAnimatable animatable, MilitaryUnit unit)
    {
        if (animatable is MGFight fight)
        {
            return fight.unit == unit || fight.unitOther == unit;
        }
        
        return animatable.unit == unit;
    }

    private void CheckQueue()
    {
        if (turnQueue.Count == 0)
        {
            Debug.LogError($"Turn Queue was empty. This shouldn't happen!");
            return;
        }

        MGTurnAnimatables animatables = turnQueue.Peek();
        if (animatables.mgAnimatables.Count == 0)
        {
            // If move queue is getting stale then lets just do something about it
            if (Time.time - timeLastMoveExecuted > 3f)
            {
                Debug.LogError($"Time in queue took an unexpectedly long time. Last move was {lastMoveExecuted}, {lastMoveExecuted.unit.UnitTeam} {lastMoveExecuted.unit.UnitType}");
                activeMoves.Clear();
            }

            // When all active moves are done, start animating the next turn
            if (activeMoves.Count == 0)
            {
                if (turnQueue.Count == 1)
                {
                    // There are no other turns in the queue! We're done here.
                    return;
                }
                else
                {
                    // Debug.Log($"Finished animating turn!");
                    turnQueue.Dequeue();
                    CheckQueue();
                    return;
                }
            }
        }

        // Try and play all moves that don't overlap in the current turn
        for (int i = 0; i < animatables.mgAnimatables.Count; i++)
        {
            IMGAnimatable animatable = animatables.mgAnimatables[i];

            if (animatable.DoesOverlapWithAny(activeMoves))
            {
                continue;
            }

            // Don't die before a unit moves to the fight
            if (animatable.DoesOverlapWithAny(animatables.mgAnimatables.GetRange(0, i)))
            {
                continue;
            }


            activeMoves.Add(animatable);
            ExecuteMove(animatable);
            animatables.mgAnimatables.RemoveAt(i);
            i -= 1;
        }

        AfterCheckQueue?.Invoke(this, new System.EventArgs());
    }

    private void ExecuteMove(IMGAnimatable move)
    {
        status = QueueStatus.Processing;
        timeLastMoveExecuted = Time.time;
        lastMoveExecuted = move;
        move.Execute(() => FinishMove(move));
        // move.unit.NPCController.AnimateMove(move, false, () => FinishMove(move));

        CoroutineUtils.ExecuteAfterDelay(
            () => {
                if (activeMoves.Contains(move))
                {
                    activeMoves.Remove(move);
                    Debug.LogWarning($"Move was in activemoves for over 10 seconds, removing it...");
                }
            },
            this,
            10
        );
    }

    private void FinishMove(IMGAnimatable move)
    {
        activeMoves.Remove(move);
        bool queueEmpty = turnQueue.Count == 1 && turnQueue.Peek().mgAnimatables.Count == 0;
        status = queueEmpty ? QueueStatus.Off : QueueStatus.Ready;
        if (activeMoves.Count == 0)
        {
            CheckQueue();
        }
        if (status == QueueStatus.Off)
        { 
            CurrentGlobalAnimationsSpeed = BaseGlobalAnimationsSpeed;
        }
    }

    public static void SpawnFightParticles(Transform transform)
    {
        Instantiate(_instance.fightParticles, transform.position, Quaternion.identity, transform);
    }

    public static void Reset() => _instance.DoReset();

    public void DoReset()
    {
        foreach (IMGAnimatable animatable in activeMoves)
        {
            if (animatable is MGFight fight)
            {
                fight.RemoveUITracker();
            }
        }

        foreach (MGTurnAnimatables turn in turnQueue)
        {
            foreach (IMGAnimatable animatable in turn.mgAnimatables)
            {
                if (animatable is MGFight fight)
                {
                    fight.RemoveUITracker();
                }
            }
        }

        activeMoves.Clear();
        turnQueue.Clear();
        CloseAndAddNewTurnToQueue();
        MGFight.numberOfActiveFights = 0;
    }

    
    public static void SpeedUpAnimations()
    {
        if (BaseGlobalAnimationsSpeed == Speed.Medium)
        {
            CurrentGlobalAnimationsSpeed = Speed.Fast;
        }
        else
        {
            CurrentGlobalAnimationsSpeed = Speed.Medium;
        }
    }

    public static void SetBaseAnimationSpeedToMedium()
    {
        BaseGlobalAnimationsSpeed = Speed.Medium;
        if (CurrentGlobalAnimationsSpeed == Speed.Slow)
        {
            CurrentGlobalAnimationsSpeed = BaseGlobalAnimationsSpeed;
        }
    }



    private bool IsNotPlayerMGMove(IMGAnimatable move)
    {
        return !(move is MGMove && move.unit.UnitTeam == MilitaryUnit.Team.Player);
    }

    private bool AreSameType(IMGAnimatable move1, IMGAnimatable move2)
    {
        return (
            (move1 is MGMove && move2 is MGMove) ||
            (move1 is MGFight && move2 is MGFight) ||
            (move1 is MGDeath && move2 is MGDeath)
        );
    }
}