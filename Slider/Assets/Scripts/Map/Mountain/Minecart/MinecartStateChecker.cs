using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MinecartStateChecker : MonoBehaviour
{
    //C: Used to check the state of the minecart and trigger certain actions
    public MinecartState targetState;
    public bool anyState = false;

    public UnityEvent OnTargetEnter;
    public UnityEvent OnTargetExit;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.GetComponent<Minecart>()){
            Minecart mc = other.GetComponent<Minecart>();
            MinecartState state = mc.mcState;
            if(mc.isMoving && (anyState || state == targetState))
                OnTargetEnter.Invoke();
        }    
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.GetComponent<Minecart>()){
            Minecart mc = other.GetComponent<Minecart>();
            MinecartState state = mc.mcState;
            if(mc.isMoving && (anyState || state == targetState))
                OnTargetExit.Invoke();
        }    
    }
}
