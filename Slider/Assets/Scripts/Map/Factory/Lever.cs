using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : ElectricalNode
{
    [SerializeField] private Animator animator;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.INPUT;

        animator ??= GetComponent<Animator>();
    }

    public void Switch()
    {
        StartCoroutine(SwitchCoroutine());
    }

    public IEnumerator SwitchCoroutine()
    {
        animator.SetTrigger("Switched");

        //L: This waits until a specific time in the animation and then sends the signal to the rest of the circuit.
        if (Powered)
        {
            yield return new WaitUntil(() =>
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                return state.IsName("Turning Off") && state.normalizedTime > 0.4f;
            });
        } else
        {
            yield return new WaitUntil(() =>
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                return state.IsName("Turning On") && state.normalizedTime > 0.6f;
            });
        }
        StartSignal(!Powered);

    }

    
}