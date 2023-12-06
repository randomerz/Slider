using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesyncItem : Item
{
    [SerializeField] private DesyncItem itemPair;
    [SerializeField] private GameObject lightning;

    private bool isItemInPast;
    private bool fromPast;
    private STile currentTile;
    private bool isDesynced;
    private DesyncItem presentItem;
    private DesyncItem pastItem;


    private void Start()
    {
        Init();
    }

    private void Init()
    {
        isItemInPast = MagiTechGrid.IsInPast(transform);
        fromPast = isItemInPast;
        currentTile = SGrid.GetSTileUnderneath(gameObject);
        if(fromPast)
        {
            pastItem = this;
            presentItem = itemPair;
        }
        else
        {
            pastItem = itemPair;
            presentItem = this;
        }
    }

    private void OnEnable()
    {
        Portal.OnTimeChange += CheckItemsOnTeleport;
        MagiTechGrid.OnDesyncStartWorld += OnDesyncStartWorld;
        MagiTechGrid.OnDesyncEndWorld += OnDesyncEndWorld;
    }

    private void OnDisable()
    {
        Portal.OnTimeChange -= CheckItemsOnTeleport;
        MagiTechGrid.OnDesyncStartWorld -= OnDesyncStartWorld;
        MagiTechGrid.OnDesyncEndWorld -= OnDesyncEndWorld;
    }

    private void Update()
    {
        UpdateCurrentTile();
    }

    public override STile DropItem(Vector3 dropLocation, System.Action callback=null)
    {
        STile tile = base.DropItem(dropLocation, callback);
        currentTile = tile;
        return tile;
    }

    private void UpdateCurrentTile()
    {
        if(PlayerInventory.GetCurrentItem() != null && PlayerInventory.GetCurrentItem().itemName == itemName)
        {
            STile tile = Player.GetInstance().GetSTileUnderneath();
            if(currentTile != tile)
            {
                currentTile = tile;
                if(isDesynced && !MagiTechGrid.IsTileDesynced(currentTile))
                {
                    //edge case: if we are carrying the present item, we must remove it from inventory and place it where the player is before disabling 
                    if(this == presentItem)
                    {
                        ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position);
                        PlayerInventory.RemoveItem();
                        transform.position = Player.GetPosition();
                        transform.SetParent(Player.GetInstance().GetSTileUnderneath().transform);
                    }
                    isDesynced = false;
                    UpdateItemPair();

                }
                else if(!isDesynced && MagiTechGrid.IsTileDesynced(currentTile))
                {
                    isDesynced = true;
                    UpdateItemPair();
                }
            }
        }
    }

    private void OnDesyncStartWorld(object sender, MagiTechGrid.OnDesyncArgs e)
    {
        isDesynced = currentTile.islandId == e.desyncIslandId;
        UpdateItemPair();
    }

    private void OnDesyncEndWorld(object sender, MagiTechGrid.OnDesyncArgs e)
    {
        isDesynced = false;
        UpdateItemPair();
    }

    // private bool CheckIfShouldBeActive()
    // {
    //     bool eitherDesynced = presentItem.isDesynced || pastItem.isDesynced;
    //     bool shouldBeActive = fromPast || eitherDesynced || isItemInPast;
    //     return shouldBeActive;
    // }

    // private void SetActiveIfShouldBe()
    // {
    //     gameObject.SetActive(CheckIfShouldBeActive());
    //     lightning.SetActive(isDesynced);
    // }

    // private void SetBothItemsActiveIfShouldBe()
    // {
    //     presentItem.SetActiveIfShouldBe();
    //     pastItem.SetActiveIfShouldBe();
    // }

    private void UpdateItemPair()
    {
        pastItem.UpdateLightning();
        bool presentShouldBeActive = presentItem.isDesynced || pastItem.isDesynced || pastItem.isItemInPast;
        presentItem.gameObject.SetActive(presentShouldBeActive);
        presentItem.UpdateLightning();
    }

    private void UpdateLightning()
    {
        lightning.SetActive(isDesynced);
    }

    private void CheckItemsOnTeleport(object sender, Portal.OnTimeChangeArgs e)
    {
        if (PlayerInventory.GetCurrentItem() != null && PlayerInventory.GetCurrentItem().name == name) 
            isItemInPast = !e.fromPast;
        
        UpdateItemPair();
    }
}
