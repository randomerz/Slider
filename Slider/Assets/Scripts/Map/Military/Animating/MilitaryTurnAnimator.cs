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
                status = QueueStatus.Processing;
                break;
            case QueueStatus.Processing:
                // Queue is processing -- do nothing!
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
    }

    private void FinishMove(IMGAnimatable move)
    {
        activeMoves.Remove(move);
        status = moveQueue.Count == 0 ? QueueStatus.Off : QueueStatus.Ready;
        CheckQueue();
    }

    public static void SpawnFightParticles(Transform transform)
    {
        Instantiate(_instance.fightParticles, transform.position, Quaternion.identity, transform);
    }
}