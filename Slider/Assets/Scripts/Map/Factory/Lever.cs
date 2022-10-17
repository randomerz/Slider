using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : ElectricalNode
{
    [SerializeField] private Animator animator;
    [SerializeField] private bool shouldSaveLeverState; // also stay powered no matter what
    public string saveLeverString;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.INPUT;

        animator ??= GetComponent<Animator>();
    }

    private void Start() 
    {
        if (shouldSaveLeverState)
        {
            if (SaveSystem.Current.GetBool(saveLeverString))
            {
                animator.SetTrigger("Switched");
                StartSignal(true);
            }
        }
    }

    public void Switch()
    {
        // StartCoroutine(SwitchCoroutine());
        SetState(!Powered);
    }

    public void SetState(bool value) {
        if (Powered == value) {
            return;
        }

        if (value) {
            StartCoroutine(TurnOn());
        } else {
            StartCoroutine(TurnOff());
        }
    }

    public IEnumerator TurnOn() {
        animator.SetTrigger("Switched");
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("Turning On") && state.normalizedTime > 0.6f;
        });

        StartSignal(true);

        if (shouldSaveLeverState) // stay powered
        {
            SaveSystem.Current.SetBool(saveLeverString, true);
        }
    }

    public IEnumerator TurnOff() {
        animator.SetTrigger("Switched");
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("Turning Off") && state.normalizedTime > 0.4f;
        });

        StartSignal(false);
    }

    // public IEnumerator SwitchCoroutine()
    // {
    //     //L: This waits until a specific time in the animation and then sends the signal to the rest of the circuit.
    //     if (Powered)
    //     {
    //         return TurnOff();
    //     } else
    //     {
    //         return TurnOn();
    //     }
    // }
}