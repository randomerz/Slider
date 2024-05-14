using System.Collections.Generic;
using UnityEngine;

public class MilitaryTurnAnimator : Singleton<MilitaryTurnAnimator>
{
    public Queue<MGMove> moveQueue = new();

    private void Awake()
    {
        InitializeSingleton();
    }

    public static void AddNewMove(MGMove move)
    {
        // _instance.moveQueue.Enqueue(move);
        _instance.ExecuteMove(move);
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
    }
}