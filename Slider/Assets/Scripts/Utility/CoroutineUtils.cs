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
    public static Coroutine ExecuteAfterDelay(Action actionToExecute, MonoBehaviour coroutineOwner, float seconds)
    {
        if (coroutineOwner != null && coroutineOwner.isActiveAndEnabled)
        {
            return coroutineOwner.StartCoroutine(IExecuteAfterDelay(actionToExecute, seconds));
        }
        return null;
    }

    private static IEnumerator IExecuteAfterDelay(Action actionToExecute, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        actionToExecute?.Invoke();
    }

    /// <summary>
    /// Wait for the end of the current frame, then invoke the passed in action.
    /// </summary>
    /// <param name="actionToExecute">The action to execute after the end of the current frame</param>
    /// <param name="coroutineOwner">We need a MonoBehaviour to start the coroutine on</param>
    public static Coroutine ExecuteAfterEndOfFrame(Action actionToExecute, MonoBehaviour coroutineOwner)
    {
        if (coroutineOwner != null && coroutineOwner.isActiveAndEnabled)
        {
            return coroutineOwner.StartCoroutine(IExecuteAfterEndOfFrame(actionToExecute));
        }
        return null;
    }

    private static IEnumerator IExecuteAfterEndOfFrame(Action actionToExecute)
    {
        yield return new WaitForEndOfFrame();
        actionToExecute?.Invoke();
    }

    /// <summary>
    /// Does a loop for duration seconds, executing the action and passing a float from 0 to 1 as time progresses. Animation curve optional.
    /// </summary>
    /// <param name="actionToExecute">The code to execute. Takes a float that will range from 0 to 1 as the loop progresses.</param>
    /// <param name="actionOnFinish">The code to execute when the loop finishes.</param>
    /// <param name="coroutineOwner">We need a MonoBehaviour to start the coroutine on</param>
    /// <param name="duration">How long to run the loop</param>
    /// <param name="curve">Optional curve to be applied to the float before it is passed.</param>
    public static Coroutine ExecuteEachFrame(Action<float> actionToExecute, Action actionOnFinish, MonoBehaviour coroutineOwner, float duration, AnimationCurve curve=null)
    {
        if (coroutineOwner != null && coroutineOwner.isActiveAndEnabled)
        {
            return coroutineOwner.StartCoroutine(IExecuteEachFrame(actionToExecute, actionOnFinish, duration, curve));
        }
        return null;
    }

    private static IEnumerator IExecuteEachFrame(Action<float> actionToExecute, Action actionOnFinish, float duration, AnimationCurve curve)
    {
        float time = 0;
        while (time < duration)
        {
            float x = time / duration;
            if (curve != null)
            {
                x = curve.Evaluate(x);
            }
            actionToExecute?.Invoke(x);

            yield return null;
            time += Time.deltaTime;
        }
        
        actionOnFinish?.Invoke();
    }
}
