using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryButton : ElectricalNode
{
    [SerializeField] private Animator animator;

    private int numObjectsOn;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.INPUT;
        numObjectsOn = 0;

        animator ??= GetComponent<Animator>();
    }
    public void Switch(bool powered)
    {
        StartSignal(powered);
        animator.SetBool("Powered", powered);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Might have to restrict this to specific items, but it works for now.
        if (other.CompareTag("Player") || other.CompareTag("ButtonTrigger"))
        {
            if (numObjectsOn == 0)
            {
                Switch(true);
            }
            numObjectsOn++;
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("ButtonTrigger"))
        {
            numObjectsOn--;
            if (numObjectsOn <= 0)
            {
                numObjectsOn = 0;
                Switch(false);
            }
        }

    }
}
