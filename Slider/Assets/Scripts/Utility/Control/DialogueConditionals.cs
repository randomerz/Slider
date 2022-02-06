using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueConditionals : Conditionals
{
    [TextArea(1, 4)]
    public string dialogue;
    public int priority;

    private int currentprio = 0;

    public new bool CheckConditions()
    {
        foreach (Condition cond in conditions)
        {
            if (!cond.CheckCondition())
            {
                onFail?.Invoke();
                currentprio = -1 * priority;
                return false;
            }
        }
        currentprio = priority;
        onSuccess?.Invoke();
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
}
