using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredDoor : ElectricalNode
{
    //Probably want to do an animation later instead of sprite swapping

    [SerializeField] private Animator animator;
    [SerializeField] private bool shouldSaveDoorState; // also stay powered no matter what
    public string saveDoorString;
    public bool forceOpen;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;

        animator ??= GetComponent<Animator>();

        if (shouldSaveDoorState)
        {
            if (SaveSystem.Current.GetBool(saveDoorString) || SaveSystem.Current.GetBool(saveDoorString + "_forceOpen"))
            {
                animator.SetBool("Powered", true);
                animator.SetBool("Skip", true);
            }
        }
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        if (shouldSaveDoorState) // stay powered
        {
            e.powered = true;
        }

        base.OnPoweredHandler(e);
        animator.SetBool("Powered", forceOpen || e.powered);

        if (shouldSaveDoorState) // stay powered
        {
            SaveSystem.Current.SetBool(saveDoorString, true);
        }
    }

    public void SetForceOpen(bool value)
    {
        forceOpen = value;
        SaveSystem.Current.SetBool(saveDoorString + "_forceOpen", forceOpen);
        if(forceOpen)
        {
            animator.SetBool("Powered", true);
        }
        else if(_isPowered == invertSignal)
        {
            animator.SetBool("Powered", false);
        }
    }
}
