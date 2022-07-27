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
        Portal.OnTimeChange += DisablePresentOnTeleport;
    }

    private void DisablePresentOnTeleport(object sender, Portal.OnTimeChangeArgs e)
    {
        if (PlayerInventory.GetCurrentItem() != null && PlayerInventory.GetCurrentItem().name == name) isItemInPast = !e.fromPast;
        //Debug.Log("isItemInPast: " + isItemInPast + " originTile: " + originTile.hasAnchor);
        if (e.fromPast)
        {
            if (!isItemInPast && !originTile.hasAnchor)
            {
                itemPair.gameObject.SetActive(false);
            }
            else itemPair.gameObject.SetActive(true);
        }
    }
}
