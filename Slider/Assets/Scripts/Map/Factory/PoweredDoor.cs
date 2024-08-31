using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredDoor : ElectricalNode
{
    //Probably want to do an animation later instead of sprite swapping

    [SerializeField] private Animator animator;
    [SerializeField] private bool shouldSaveDoorState; // also stay powered no matter what
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D myCollider;
    public string saveDoorString;
    public bool shouldUpdateSpriteOrder;
    public bool forceOpen;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;

        animator ??= GetComponent<Animator>();

        if (myCollider == null)
        {
            if (!TryGetComponent<BoxCollider2D>(out myCollider))
            {
                Debug.LogError($"Couldn't find collider on door {name}!");
            }
        }

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

    // Handled by animator
    public void SetCollider(bool value)
    {
        if (myCollider == null)
        {
            myCollider = GetComponent<BoxCollider2D>();
        }

        if (myCollider != null)
        {
            myCollider.enabled = value;
            
            if (shouldUpdateSpriteOrder)
            {
                // Entity to -1
                spriteRenderer.sortingOrder = value ? 0 : -1;
            }
        }
    }
    public void SetColliderOn() => SetCollider(true);
    public void SetColliderOff() => SetCollider(false);
}
