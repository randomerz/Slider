using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChadSrElectricPuzzle : MonoBehaviour
{
    [SerializeField] private Item itemHolding;
    [SerializeField] private ConductiveElectricalNode itemHoldingNode;
    [SerializeField] private Collider2D itemElectricalTrigger;
    [SerializeField] private Transform dropItemLoc;

    private void OnEnable()
    {
        itemHoldingNode?.OnPoweredOn.AddListener(DropItemHolding);
    }

    private void OnDisable()
    {
        itemHoldingNode?.OnPoweredOn.RemoveListener(DropItemHolding);
    }

    private void Start()
    {
        itemElectricalTrigger.enabled = false;
        itemHolding?.SetCollider(false);
        itemHolding?.OnEquip();
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
        itemElectricalTrigger.enabled = true;
        itemHolding?.SetCollider(true);
        itemHolding.saveString = "factoryChadSrBob";
        itemHolding = null;
        itemHoldingNode = null;
        SaveSystem.Current.SetBool("ChadSrPuzzleComplete", true);
    }

    public void CheckPuzzleComplete(Condition cond)
    {
        cond.SetSpec(SaveSystem.Current.GetBool("ChadSrPuzzleComplete"));
    }
}
