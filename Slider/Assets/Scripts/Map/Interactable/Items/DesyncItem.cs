using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesyncItem : Item
{
    [SerializeField] private Sprite pastSprite;
    [SerializeField] private Sprite presentSprite;
    [SerializeField] private Item itemPair;
    [SerializeField] private GameObject lightning;
    private bool isItemInPast;
    private STile originTile;
    private bool isDesynced;

    public bool IsDesynced { get => isDesynced; }


    private void Start()
    {
        isItemInPast = MagiTechGrid.IsInPast(transform);
        originTile = SGrid.GetStileUnderneath(gameObject);
    }

    private void OnEnable()
    {
        Portal.OnTimeChange += CheckItemsOnTeleport;
        Anchor.OnAnchorInteract += CheckItemsOnAnchorInteract;
    }

    private void OnDisable()
    {
        Portal.OnTimeChange -= CheckItemsOnTeleport;
        Anchor.OnAnchorInteract -= CheckItemsOnAnchorInteract;
    }

    private void CheckItemsOnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs e)
    {
        isDesynced = e.drop && originTile != null && originTile.hasAnchor;
        itemPair.gameObject.SetActive(isItemInPast || IsDesynced);
        lightning.SetActive(IsDesynced);
    }

    private void CheckItemsOnTeleport(object sender, Portal.OnTimeChangeArgs e)
    {
        if (PlayerInventory.GetCurrentItem() != null && PlayerInventory.GetCurrentItem().name == name) isItemInPast = !e.fromPast;
        //Debug.Log("isItemInPast: " + isItemInPast + " originTile: " + originTile.hasAnchor);
        if (!isItemInPast && !originTile.hasAnchor)
        {
            itemPair.gameObject.SetActive(false);
        }
        else itemPair.gameObject.SetActive(true);
    }
}
