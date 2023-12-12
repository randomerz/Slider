using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineUtils
{
    /// <summary>
    /// Wait for the end of the current frame, then invoke the passed in action.
    /// </summary>
    /// <param name="actionToExecute">The action to execute after the end of the current frame</param>
    /// <param name="coroutineOwner">We need a MonoBehaviour to start the coroutine on</param>
    public static void ExecuteAfterEndOfFrame(Action actionToExecute, MonoBehaviour coroutineOwner)
    {
        if (coroutineOwner != null && coroutineOwner.isActiveAndEnabled)
        {
            coroutineOwner.StartCoroutine(IExecuteAfterEndOfFrame(actionToExecute));
        }
    }

    private static IEnumerator IExecuteAfterEndOfFrame(Action actionToExecute)
    {
        yield return new WaitForEndOfFrame();
        actionToExecute?.Invoke();
    }
}
