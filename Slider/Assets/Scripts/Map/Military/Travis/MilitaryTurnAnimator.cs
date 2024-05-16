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

    private Queue<MGMove> moveQueue = new();
    private List<MGMove> activeMoves = new();
    private QueueStatus status = QueueStatus.Off;

    private void Awake()
    {
        InitializeSingleton();
    }

    public static void AddNewMove(MGMove move)
    {
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
                MGMove nextMove = moveQueue.Dequeue();
                activeMoves.Add(nextMove);
                ExecuteMove(nextMove);
                status = QueueStatus.Processing;
                break;
            case QueueStatus.Processing:
                // Queue is processing -- do nothing!
                break;
        }
    }

    private void ExecuteMove(MGMove move)
    {
        Vector3 targetPos;
        if (move.endStile == null)
        {
            targetPos = MilitaryUnit.GridPositionToWorldPosition(move.endCoords);
        }
        else
        {
            targetPos = move.endStile.transform.position;
        }

        move.unit.NPCController.SetPosition(targetPos);
        move.unit.AttachedSTile = move.endStile;

        FinishMove(move);
    }

    private void FinishMove(MGMove move)
    {
        activeMoves.Remove(move);
        status = moveQueue.Count == 0 ? QueueStatus.Off : QueueStatus.Processing;
    }
}