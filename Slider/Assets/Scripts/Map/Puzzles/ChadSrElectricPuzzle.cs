using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChadSrElectricPuzzle : MonoBehaviour
{
    private const string CHAD_SR_INTRO = "FactoryPastChadSrIntro";
    private const string CHAD_SR_MOVED_TILE = "FactoryPastChadSrMovedTile";

    [SerializeField] private Item itemHolding;
    [SerializeField] private Transform itemHolderTransform;
    [SerializeField] private ConductiveElectricalNode itemHoldingNode;
    [SerializeField] private Collider2D itemElectricalTrigger;
    [SerializeField] private Transform dropItemLoc;

    private void OnEnable()
    {
        itemHoldingNode?.OnPoweredOn.AddListener(DropItemHolding);
        SGridAnimator.OnSTileMoveEnd += OnSTileMove;
    }

    private void OnDisable()
    {
        itemHoldingNode?.OnPoweredOn.RemoveListener(DropItemHolding);
        SGridAnimator.OnSTileMoveEnd -= OnSTileMove;
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

    private void OnSTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (SaveSystem.Current.GetBool(CHAD_SR_INTRO))
        {
            SaveSystem.Current.SetBool(CHAD_SR_MOVED_TILE, true);
        }
    }
}
