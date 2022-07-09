using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Conditionals
{
    [Header("Base Condition Events (DO NOT USE FOR DIALOGUE)")]
    public UnityEvent onSuccess;
    public UnityEvent onFail;
    public List<Condition> conditions;
    public bool CheckConditions() 
    {
        foreach (Condition cond in conditions) 
        {
            if (!cond.CheckCondition())
            {
                onFail?.Invoke();
                return false;
            }
        }
        onSuccess?.Invoke();
        return true;
    }
}
