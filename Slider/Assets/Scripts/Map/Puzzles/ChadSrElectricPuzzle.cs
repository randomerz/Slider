using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChadSrElectricPuzzle : MonoBehaviour
{
    [SerializeField] private Item itemHolding;
    [SerializeField] private Transform itemHolderTransform;
    [SerializeField] private ConductiveElectricalNode itemHoldingNode;
    [SerializeField] private Collider2D itemElectricalTrigger;
    [SerializeField] private Transform dropItemLoc;

    private bool updateTransformLateStart;

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
        if (!SaveSystem.Current.GetBool("factoryPastChadDroppedItem"))
        {
            itemElectricalTrigger.enabled = false;
            itemHolding.SetCollider(false);
            itemHolding.OnEquip();
    
            // Item has to update transform/position in Start() because it doesn't have access to SGrid functions in Load()
            StartCoroutine(StartLate());
        }
    }

    private IEnumerator StartLate()
    {
        yield return new WaitForEndOfFrame();

        itemHolding.transform.SetParent(itemHolderTransform);
        itemHolding.transform.position = itemHolderTransform.position;
    }

    private void DropItemHolding()
    {
        if (itemHolding != null)
        {
            itemHolding.DropItem(dropItemLoc.position, callback: FinishItemDrop);
            itemHolding.SetCollider(false);
            itemHoldingNode.OnPoweredOn.RemoveListener(DropItemHolding);

            SaveSystem.Current.SetBool("factoryPastChadDroppedItem", true);
        }
    }

    private void FinishItemDrop()
    {
        itemElectricalTrigger.enabled = true;
        itemHolding?.SetCollider(true);
        itemHolding = null;
        itemHoldingNode = null;
        SaveSystem.Current.SetBool("ChadSrPuzzleComplete", true);
    }

    public void CheckPuzzleComplete(Condition cond)
    {
        cond.SetSpec(SaveSystem.Current.GetBool("ChadSrPuzzleComplete"));
    }
}
