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
    public int GetPrio()
    {
        return currentprio;
    }
    public void OnDialogue()
    {
        onDialogue?.Invoke();
    }
    public void SetPrio(int p)
    {
        priority = p;
    }
    public void KillDialogue()
    {
        priority = 0;
    }
}
