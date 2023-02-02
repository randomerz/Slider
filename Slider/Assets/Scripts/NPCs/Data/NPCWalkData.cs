using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class NPCWalkData
{
    public List<Transform> path;
    public List<STileCrossing> stileCrossings;
    public bool turnAroundAfterWalking;
    public bool teleportToEndIfInterrupted;

    public UnityEvent onPathStarted;
    public UnityEvent onPathFinished;
    public UnityEvent onPathBroken;
    public UnityEvent onPathResumed;
}
