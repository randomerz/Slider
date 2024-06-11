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
    private List<IMGAnimatable> activeMoves = new();
    private QueueStatus status = QueueStatus.Off;

    private float timeLastMoveExecuted;
    private IMGAnimatable lastMoveExecuted;
    
    [SerializeField] private GameObject fightParticles;

    private void Awake()
    {
        InitializeSingleton();
    }

    public static void AddToQueue(IMGAnimatable move)
    {
        _instance.moveQueue.Enqueue(move);
        _instance.CheckQueue();
    }

    public static void AddToQueueFront(IMGAnimatable move)
    {
        // TODO: implement
        // - i dont think we actually want an add to front of queue though
        //   since all of the actions get put into the queue immediately.
        //   we want something to attach to after a units move
        _instance.moveQueue.Enqueue(move);
        _instance.CheckQueue();
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

                // If next move is an alien move, play all alien moves at the front of the queue
                while (moveQueue.Count > 0 && IsAlienMGMove(nextMove))
                {
                    nextMove = moveQueue.Dequeue();
                    if (nextMove != null && IsAlienMGMove(nextMove))
                    {
                        activeMoves.Add(nextMove);
                        ExecuteMove(nextMove);
                    }
                    else
                    {
                        break;
                    }
                }

                break;

            case QueueStatus.Processing:
                // Queue is processing!

                // If new and all active moves are alien MGMoves, play the new one too
                if (moveQueue.Count > 0 && IsAlienMGMove(moveQueue.Peek()))
                {
                    if (activeMoves.Count > 0 && AreAllActiveMovesAlienMGMove())
                    {
                        IMGAnimatable topMove = moveQueue.Dequeue();
                        activeMoves.Add(topMove);
                        ExecuteMove(topMove);
                    }
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


    private bool IsAlienMGMove(IMGAnimatable move)
    {
        return move is MGMove && (move as MGMove).unit.UnitTeam == MilitaryUnit.Team.Alien;
    }

    private bool AreAllActiveMovesAlienMGMove()
    {
        foreach (IMGAnimatable m in activeMoves)
        {
            if (!IsAlienMGMove(m))
            {
                return false;
            }
        }
        return true;
    }
}