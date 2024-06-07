using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesyncItem : Item
{
    [SerializeField] private DesyncItem itemPair;
    [SerializeField] private GameObject lightning;
    [SerializeField] private Sprite trackerSprite;
    [SerializeField] private LayerMask noDropItemsMask;

    private bool isItemInPast;
    private bool fromPast;
    private STile currentTile;
    private bool isDesynced;
    private DesyncItem presentItem;
    private DesyncItem pastItem;
    private bool isTracked;
    private bool itemDoesNotExist;

    private bool didInit;
    public LayerMask blocksSpawnMask;
    public GameObject particles;


    public override void Start()
    {
        base.Start();

        Init();

        if (saveString == "" && trackerSprite != null)
        {
            Debug.LogWarning("Save string for item was empty, but tracker sprite was not. Is this intended?");
        }

        StartCoroutine(LateStart());
    }

    // The position/collider updates in Load() don't register until the end of frame
    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();

        // For checking if item spawns in a portal bc of save/load or scene change
        MoveIfInIllegalBounds();
    }

    private void Init()
    {
        if (didInit)
            return;

        didInit = true;
        isItemInPast = MagiTechGrid.IsInPast(transform);
        fromPast = isItemInPast;
        currentTile = SGrid.GetSTileUnderneath(gameObject, includeInactive: true);
        if (fromPast)
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
        Init();
        base.Load(profile);
        if (saveString != null && saveString != "" && profile.GetBool($"{saveString}_IsTracked"))
        {
            AddTracker();
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

    public override void Update()
    {
        base.Update();
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
        if(ShouldMovePresentItem())
        {
            MovePresentItemToPastLocation();
        }
        return tile;
    }

    public override void PickUpItem(Transform pickLocation, System.Action callback = null) 
    {
        if(ShouldMovePresentItem())
        {
            MovePresentItemToPastLocation();
        }
        base.PickUpItem(pickLocation, callback);
        if (trackerSprite)
        {
            UITrackerManager.RemoveTracker(gameObject);
        }
    }

    private bool ShouldMovePresentItem()
    {
        return fromPast && pastItem.isItemInPast && !presentItem.isDesynced && ! pastItem.isDesynced;
    }

    private void MovePresentItemToPastLocation()
    {
        Vector3 pastLocalLoc = transform.localPosition;
        Vector3 checkPos;
        STile presentTile = null;
        if(currentTile != null)
        {
            presentTile = MagiTechGrid.Instance.FindAltStile(currentTile);
            checkPos = presentTile.transform.position + pastLocalLoc;
            if(presentTile.islandId == 3)
            {
                checkPos += new Vector3(0, -150f, 0);
            }
        }
        else
        {
            checkPos = transform.localPosition + new Vector3(-100f, 0, 0);
            checkPos.x = Mathf.Clamp(checkPos.x, -9f, 43f);
            checkPos.y = Mathf.Clamp(checkPos.y, -16f, 43f);
        }
        Vector3 targetPos = ItemPlacerSolver.FindItemPlacePosition(checkPos, 9, blocksSpawnMask, true);
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, presentItem.transform.position);
        if(targetPos.x == float.MaxValue)
        {
            Debug.LogWarning("Could not find valid position for present item. Moving anyways");
        }
        presentItem.transform.position = targetPos;
        if(presentTile != null)
        {
            presentItem.transform.parent = presentTile.transform;
        }
        else
        {
            presentItem.transform.parent = null;
        }
    }

    public void MoveIfInIllegalBounds()
    {
        // May want to change this to handle multiple collisions
        Collider2D otherCollider = Physics2D.OverlapCircle(transform.position, itemRadius, noDropItemsMask);

        if (otherCollider != null)
        {
            bool wasColliderOn = myCollider.enabled;
            SetCollider(true);
            ColliderDistance2D colliderDistance = myCollider.Distance(otherCollider);
            SetCollider(wasColliderOn);

            if (colliderDistance.isValid && colliderDistance.isOverlapped)
            {
                Vector3 offset = colliderDistance.normal * colliderDistance.distance;
                Debug.Log($"Collider was in illegal bounds. Moving from {myCollider.gameObject.transform.position} in direction {offset}");
                myCollider.gameObject.transform.position += offset;
                return;
            }
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
        if (Portal.playerInPortal && Portal.recentPortalObj != null)
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

    public void SetIsTracked(bool val)
    {
        isTracked = val;
        if (isTracked)
            AddTracker();
        else
            UITrackerManager.RemoveTracker(gameObject);
    }

    private void AddTracker()
    {
        if (trackerSprite != null)
            UITrackerManager.AddNewTracker(gameObject, trackerSprite);
        else
            UITrackerManager.AddNewTracker(gameObject);
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
        particles.SetActive(active);
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
