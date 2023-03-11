using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class InvokeAfterDelay : MonoBehaviour
{
    public UnityEvent EventToInvoke;

    public void InvokeAfterSeconds(float seconds)
    {
        StartCoroutine(IInvokeAfterSeconds(seconds));
    }

    private IEnumerator IInvokeAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        EventToInvoke.Invoke();
    }
}