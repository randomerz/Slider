using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesyncItem : Item
{
    [SerializeField] private DesyncItem itemPair;
    [SerializeField] private GameObject lightning;

    private bool isItemInPast;
    private bool fromPast;
    private STile originTile;
    private bool isDesynced;

    public bool IsDesynced { get => isDesynced; }


    private void Start()
    {
        isItemInPast = MagiTechGrid.IsInPast(transform);
        fromPast = isItemInPast;
        originTile = SGrid.GetSTileUnderneath(gameObject);
    }

    private void OnEnable()
    {
        Portal.OnTimeChange += CheckItemsOnTeleport;
       // Anchor.OnAnchorInteract += CheckItemsOnAnchorInteract;
        MagiTechGrid.OnDesyncStartWorld += OnDesyncStartWorld;
        MagiTechGrid.OnDesyncEndWorld += OnDesyncEndWorld;
    }

    private void OnDisable()
    {
        Portal.OnTimeChange -= CheckItemsOnTeleport;
       // Anchor.OnAnchorInteract -= CheckItemsOnAnchorInteract;
        MagiTechGrid.OnDesyncStartWorld -= OnDesyncStartWorld;
        MagiTechGrid.OnDesyncEndWorld -= OnDesyncEndWorld;
    }

    // private void CheckItemsOnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs e)
    // {
    //     isDesynced = e.drop && originTile != null && originTile.hasAnchor;
    //     itemPair.gameObject.SetActive(isItemInPast || IsDesynced);
    //     lightning.SetActive(IsDesynced);
    // }

    private void OnDesyncStartWorld(object sender, MagiTechGrid.OnDesyncArgs e)
    {
        isDesynced = originTile.islandId == e.desyncIslandId;
        gameObject.SetActive(isItemInPast || IsDesynced || fromPast);
        lightning.SetActive(IsDesynced);
    }

    private void OnDesyncEndWorld(object sender, MagiTechGrid.OnDesyncArgs e)
    {
        isDesynced = false;
        gameObject.SetActive(isItemInPast || IsDesynced || fromPast);
        lightning.SetActive(IsDesynced);
    }

    private void CheckItemsOnTeleport(object sender, Portal.OnTimeChangeArgs e)
    {
        if (PlayerInventory.GetCurrentItem() != null && PlayerInventory.GetCurrentItem().name == name) 
            isItemInPast = !e.fromPast;
        
        bool eitherDesynced = MagiTechGrid.IsTileDesynced(originTile) || MagiTechGrid.IsTileDesynced(itemPair.originTile);
        
        if(eitherDesynced || isItemInPast || itemPair.fromPast)
        {
            itemPair.gameObject.SetActive(true);
        }
        else
        {
            itemPair.gameObject.SetActive(false);
        }
        //Debug.Log("isItemInPast: " + isItemInPast + " originTile: " + originTile.hasAnchor);
        // if (!isItemInPast && !originTile.hasAnchor)
        // {
        //     itemPair.gameObject.SetActive(false);
        // }
        // else itemPair.gameObject.SetActive(true);
    }
}
