using System.Collections.Generic;
using UnityEngine;

public class MilitaryTurnAnimator : Singleton<MilitaryTurnAnimator>
{
    public enum QueueStatus 
    {
        Off,
        Ready, // Might still be processing but send the next
        Processing, // Proccessing animations
    }

    private Queue<IMGAnimatable> moveQueue = new();

    private Queue<IMGAnimatable> moveBuffer = new();
    private Queue<IMGAnimatable> fightBuffer = new();
    private Queue<IMGAnimatable> deathBuffer = new();

    private Coroutine updateBuffersCoroutine;
    private List<IMGAnimatable> activeMoves = new();
    private QueueStatus status = QueueStatus.Off;

    private float timeLastMoveExecuted;
    private IMGAnimatable lastMoveExecuted;
    
    [SerializeField] private GameObject fightParticles;

    private void Awake()
    {
        InitializeSingleton();
    }

    public static void AddToQueue(IMGAnimatable move) => _instance._AddToQueue(move);

    public void _AddToQueue(IMGAnimatable move)
    {
        // _instance.moveQueue.Enqueue(move);
        // _instance.CheckQueue();
        if (move is MGMove)
        {
            moveBuffer.Enqueue(move);
        }
        else if (move is MGFight)
        {
            fightBuffer.Enqueue(move);
        }
        else
        {
            deathBuffer.Enqueue(move);
        }

        if (updateBuffersCoroutine == null)
        {
            updateBuffersCoroutine = CoroutineUtils.ExecuteAfterEndOfFrame(
                () => {
                    while (moveBuffer.Count > 0)
                    {
                        moveQueue.Enqueue(moveBuffer.Dequeue());
                    }
                    while (fightBuffer.Count > 0)
                    {
                        moveQueue.Enqueue(fightBuffer.Dequeue());
                    }
                    while (deathBuffer.Count > 0)
                    {
                        moveQueue.Enqueue(deathBuffer.Dequeue());
                    }
                    CheckQueue();
                    updateBuffersCoroutine = null;
                }, this
            );
        }
    }

    private void CheckQueue()
    {
        if (moveQueue.Count == 0)
        {
            return;
        }

        switch (status)
        {
            case QueueStatus.Off:
            case QueueStatus.Ready:
                IMGAnimatable nextMove = moveQueue.Dequeue();
                activeMoves.Add(nextMove);
                ExecuteMove(nextMove);

                // If next move is not player movement, try to play all of the same type at same time
                while (moveQueue.Count > 0 && IsNotPlayerMGMove(nextMove))
                {
                    IMGAnimatable newestMove = moveQueue.Peek();
                    if (AreSameType(newestMove, nextMove))
                    {
                        moveQueue.Dequeue();
                        activeMoves.Add(newestMove);
                        ExecuteMove(newestMove);
                    }
                    else
                    {
                        break;
                    }
                }

                break;

            case QueueStatus.Processing:
                // Queue is processing!

                // Try to play all of the same type at same time
                if (moveQueue.Count > 0 && AreSameType(moveQueue.Peek(), lastMoveExecuted) && IsNotPlayerMGMove(moveQueue.Peek()))
                {
                    IMGAnimatable topMove = moveQueue.Dequeue();
                    activeMoves.Add(topMove);
                    ExecuteMove(topMove);
                }

                // if move queue is getting stale then lets just do something about it
                if (Time.time - timeLastMoveExecuted > 2)
                {
                    if (lastMoveExecuted is MGMove)
                    {
                        Debug.LogError($"Time in queue took an unexpectedly long time. Last move was {(lastMoveExecuted as MGMove).unit.UnitTeam} {(lastMoveExecuted as MGMove).unit.UnitType}");
                    }
                    Debug.Log($"Allowing queue to process more.");
                    status = QueueStatus.Ready;
                    CheckQueue();
                }

                break;
        }
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
        status = moveQueue.Count == 0 ? QueueStatus.Off : QueueStatus.Ready;
        if (activeMoves.Count == 0)
        {
            CheckQueue();
        }
    }

    public static void SpawnFightParticles(Transform transform)
    {
        Instantiate(_instance.fightParticles, transform.position, Quaternion.identity, transform);
    }


    private bool IsAlienMove(IMGAnimatable move)
    {
        return move.unit.UnitTeam == MilitaryUnit.Team.Alien;
        // return move is MGMove && (move as MGMove).unit.UnitTeam == MilitaryUnit.Team.Alien;
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

    private bool AreAllActiveMovesAlienMGMove()
    {
        foreach (IMGAnimatable m in activeMoves)
        {
            if (!IsAlienMove(m))
            {
                return false;
            }
        }
        return true;
    }
}