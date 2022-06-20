using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredDoor : ElectricalNode
{
    //Probably want to do an animation later instead of sprite swapping

    [SerializeField] private Animator animator;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;

        animator ??= GetComponent<Animator>();
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        //Debug.Log($"We Powered? {e.powered}");
        base.OnPoweredHandler(e);
        animator.SetBool("Powered", e.powered);
    }
}
