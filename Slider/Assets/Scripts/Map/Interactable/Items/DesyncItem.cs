using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesyncItem : Item
{
    [SerializeField] private Sprite pastSprite;
    [SerializeField] private Sprite presentSprite;
    [SerializeField] private Item itemPair;

    private bool isItemInPast;
    private STile originTile;

    private void Start()
    {
        isItemInPast = transform.position.x > 67;
        originTile = SGrid.GetStileUnderneath(gameObject);
    }

    private void OnEnable()
    {
        Portal.OnTimeChange += CheckItemsOnTeleport;
        Anchor.OnAnchorInteract += CheckItemsOnAnchorInteract;
    }

    private void CheckItemsOnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs e)
    {
        if (!isItemInPast && !originTile.hasAnchor)
        {
            itemPair.gameObject.SetActive(false);
        }
        else itemPair.gameObject.SetActive(true);
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
