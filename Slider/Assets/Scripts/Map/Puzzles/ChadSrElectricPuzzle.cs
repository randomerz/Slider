using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChadSrElectricPuzzle : MonoBehaviour
{
    [SerializeField] private Item itemHolding;
    [SerializeField] private ConductiveElectricalNode itemHoldingNode;
    [SerializeField] private Transform dropItemLoc;

    private void OnEnable()
    {
        itemHoldingNode.OnPoweredOn.AddListener(DropItemHolding);
    }

    private void OnDisable()
    {
        itemHoldingNode.OnPoweredOn.RemoveListener(DropItemHolding);
    }

    void Start()
    {
        itemHolding?.SetCollider(false);
    }

    private void DropItemHolding()
    {
        if (itemHolding != null)
        {
            itemHolding.DropItem(dropItemLoc.position, callback: FinishItemDrop);
            itemHolding.SetCollider(false);
            itemHoldingNode.OnPoweredOn.RemoveListener(DropItemHolding);
        }
    }

    private void FinishItemDrop()
    {
        itemHolding?.SetCollider(true);
        itemHolding = null;
        itemHoldingNode = null;
    }
}
