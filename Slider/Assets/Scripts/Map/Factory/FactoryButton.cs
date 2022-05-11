using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryButton : ElectricalNode
{
    [SerializeField]
    private GameObject offSprite;
    [SerializeField]
    private GameObject onSprite;

    private int numObjectsOn;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.INPUT;
        numObjectsOn = 0;
    }

    public void Switch()
    {
        StartSignal(!Powered);
        offSprite.SetActive(!Powered);
        onSprite.SetActive(Powered);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Might have to restrict this to specific items, but it works for now.
        if (collision.CompareTag("Player") || collision.CompareTag("Item"))
        {
            if (numObjectsOn == 0)
            {
                Switch();
            }
            numObjectsOn++;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Item"))
        {
            numObjectsOn--;
            if (numObjectsOn <= 0)
            {
                numObjectsOn = 0;
                Switch();
            }
        }

    }
}
