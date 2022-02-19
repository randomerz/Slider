using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Node
{
    public enum NodeState
    {
        SUCCESS,
        FAILURE,
        RUNNING
    }

    protected NodeState _state;

    public NodeState State { get { return _state; } }

    public abstract NodeState Evaluate();
}
