using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesyncItem : Item
{
    [SerializeField] private DesyncItem itemPair;
    [SerializeField] private GameObject lightning;
    [SerializeField] private Sprite trackerSprite;

    private bool isItemInPast;
    private bool fromPast;
    private STile currentTile;
    private bool isDesynced;
    private DesyncItem presentItem;
    private DesyncItem pastItem;
    private bool isTracked;
    private bool itemDoesNotExist;


    private void Start()
    {
        Init();

        if (saveString == "" && trackerSprite != null)
        {
            Debug.LogWarning("Save string for item was empty, but tracker sprite was not. Is this intended?");
        }
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
        CheckDesyncOnEnable();
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
        if(isTracked)
        {
            AddTracker();
        }
        return tile;
    }

    public void SetIsTracked(bool val)
    {
        isTracked = val;
        if(isTracked)
            AddTracker();
        else
            UITrackerManager.RemoveTracker(gameObject);
    }

    private void AddTracker()
    {
        if(trackerSprite != null)
            UITrackerManager.AddNewTracker(gameObject, trackerSprite);
        else
            UITrackerManager.AddNewTracker(gameObject);
    }

    public override void PickUpItem(Transform pickLocation, System.Action callback = null) 
    {
        base.PickUpItem(pickLocation, callback);
        if (trackerSprite)
        {
            UITrackerManager.RemoveTracker(gameObject);
        }
    }

    public override void Save()
    {
        base.Save();
        if (saveString != null && saveString != "")
        {
            SaveSystem.Current.SetBool($"{saveString}_IsTracked", isTracked);
        }
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        if (saveString != null && saveString != "" && profile.GetBool($"{saveString}_IsTracked"))
        {
            AddTracker();
        }
    }

    private void UpdateCurrentTile()
    {
        if(PlayerInventory.GetCurrentItem() != null && PlayerInventory.GetCurrentItem().itemName == itemName)
        {
            STile tile = Player.GetInstance().GetSTileUnderneath();
            if(currentTile != tile)
            {
                currentTile = tile;
                UpdateDesyncOnChangeTile();
            }
        }
    }

    private void CheckDesyncOnEnable()
    {
        if(!isDesynced && MagiTechGrid.IsTileDesynced(currentTile))
        {
            isDesynced = true;
            UpdateItemPair();
        }
    }

    private void UpdateDesyncOnChangeTile()
    {
        if(isDesynced && !MagiTechGrid.IsTileDesynced(currentTile))
        {
            isDesynced = false;
            //edge case: if we are carrying the present item and the past item isn't in the past or desynced, we must remove it from inventory and place it where the player is before disabling 
            if(this == presentItem && !(pastItem.isDesynced || pastItem.isItemInPast))
            {
                RemoveItemFromPlayerInventory();
            }
            else
            {
                UpdateItemPair();
            }
        }
        else if(!isDesynced && MagiTechGrid.IsTileDesynced(currentTile))
        {
            isDesynced = true;
            UpdateItemPair();
        }
    }

    private void RemoveItemFromPlayerInventory()
    {
        PlayerInventory.RemoveItem();
        //Make sure it doesn't end up inside the portal
        if(Portal.playerInPortal && Portal.recentPortalObj != null)
        {
            transform.position = Portal.recentPortalObj.desyncItemFallbackSpawn.position; 
        }
        else
        {
            transform.position = Player.GetPosition();
        }
        transform.SetParent(Player.GetInstance().GetSTileUnderneath().transform);
        SetLayer(LayerMask.NameToLayer("Item"));
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position);
        ResetSortingOrder();
        SetDesyncItemActive(false);
        UpdateLightning();
    }

    private void OnDesyncStartWorld(object sender, MagiTechGrid.OnDesyncArgs e)
    {
        isDesynced = currentTile.islandId == e.desyncIslandId || currentTile.islandId == e.anchoredTileIslandId;
        UpdateItemPair();
    }

    private void OnDesyncEndWorld(object sender, MagiTechGrid.OnDesyncArgs e)
    {
        isDesynced = false;
        UpdateItemPair();
    }

    private void UpdateItemPair(bool fromPortal = false)
    {
        pastItem.UpdateLightning();
        bool presentShouldBeActive = presentItem.isDesynced || pastItem.isDesynced || pastItem.isItemInPast;
        if(presentItem.itemDoesNotExist == presentShouldBeActive) //these should normally be opposite, IE if present doesn't exist but should be active, then we must update the present item state
        {   
            if(fromPortal 
            && PlayerInventory.GetCurrentItem() != null 
            && PlayerInventory.GetCurrentItem().itemName == itemName
            && this == presentItem)
            {
                RemoveItemFromPlayerInventory();
            }
            else
            {
                presentItem.SetDesyncItemActive(presentShouldBeActive);
                ParticleManager.SpawnParticle(ParticleType.SmokePoof, presentItem.transform.position);
            }
        }
        presentItem.UpdateLightning();
    }

    private void SetDesyncItemActive(bool active)
    {
        itemDoesNotExist = !active;
        spriteRenderer.enabled = active;
        myCollider.enabled = active;
        if(isTracked)
        {
            UITrackerManager.RemoveTracker(gameObject);
        }
    }

    private void UpdateLightning()
    {
        lightning.SetActive(isDesynced);
    }

    private void CheckItemsOnTeleport(object sender, Portal.OnTimeChangeArgs e)
    {
        if (PlayerInventory.GetCurrentItem() != null && PlayerInventory.GetCurrentItem().name == name) 
            isItemInPast = !e.fromPast;
        UpdateItemPair(true);
    }
}
