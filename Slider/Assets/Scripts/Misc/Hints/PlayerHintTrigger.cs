using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerHintTrigger : MonoBehaviour
{
    public UnityEvent ArtifactOpen;
    public UnityEvent PlayerMove;
    public UnityEvent PlayerAction;
    public UnityEvent PlayerCycle;

    public void OnOpenArtifact(InputValue context)
    {
        ArtifactOpen?.Invoke();
    }

    public void OnAction(InputValue context)
    {
        PlayerAction?.Invoke();
    }

    public void OnCycleEquip(InputValue context)
    {
        PlayerCycle?.Invoke();
    }
    
    public void OnMove(InputValue context)
    {
        PlayerMove?.Invoke();
    }
}
