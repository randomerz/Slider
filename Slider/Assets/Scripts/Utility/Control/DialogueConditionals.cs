using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueConditionals : Conditionals
{
    [TextArea(1, 4)]
    public string dialogue;
    public int priority;
    public UnityEvent onDialogue;

    private int currentprio = 0;

    public new bool CheckConditions()
    {
        foreach (Condition cond in conditions)
        {
            if (!cond.CheckCondition())
            {
                onSuccess?.Invoke();
                currentprio = -1 * priority;
                return false;
            }
        }
        onFail?.Invoke();
        currentprio = priority;
        return true;
    }
    public string GetDialogue()
    {
        return dialogue;
    }
    public int GetPriority()
    {
        return currentprio;
    }
    public void OnDialogue()
    {
        onDialogue?.Invoke();
    }
    public void ClearPrio()
    {
        currentprio = -1 * priority;
    }
}
