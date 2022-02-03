using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Conditionals
{
    public UnityEvent onSuccess;
    public List<Condition> conditions;
    public bool CheckConditions() 
    {
        foreach (Condition cond in conditions) 
        {
            if (!cond.CheckCondition())
            {
                return false;
            }
        }
        onSuccess?.Invoke();
        return true;
    }
}
