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
    public bool fromPast;
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
        
        isItemInPast = MagiTechGrid.IsInPast(transform);
        MoveIfInIllegalBounds();
        UpdateItemPair();
    }

    private void Init()
    {
        if (didInit)
            return;

        didInit = true;
        isItemInPast = MagiTechGrid.IsInPast(transform);
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
        System.Action newCallback = () => {
            callback();
            UpdateDesyncOnChangeTile();
        };
        STile tile = base.DropItem(dropLocation, newCallback);
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
        SetMoved();
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

    private void SetMoved()
    {
        SaveSystem.Current.SetBool($"{saveString}_Moved", true);
    }

    private bool GetMoved()
    {
        return SaveSystem.Current.GetBool($"{saveString}_Moved");
    }

    private bool ShouldMovePresentItem()
    {
        return fromPast && GetMoved() && pastItem.isItemInPast && !presentItem.isDesynced && !pastItem.isDesynced;
    }

    private void MovePresentItemToPastLocation()
    {
        Vector3 pastLocalLoc = GetLocalPosition();
        Vector3 checkPos;
        STile presentTile = null;
        currentTile = SGrid.GetSTileUnderneath(gameObject, includeInactive: true);
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
            checkPos = transform.position + new Vector3(-100f, 0, 0);
            checkPos.x = Mathf.Clamp(checkPos.x, -9f, 43f);
            checkPos.y = Mathf.Clamp(checkPos.y, -16f, 43f);
            if(checkPos.x < -8f)
                checkPos.x = -9f;
            if(checkPos.x > 42f)
                checkPos.x = 43f;
            if(checkPos.y > 42f)
                checkPos.y = 43f;
        }
        presentItem.isItemInPast = false;
        Vector3 targetPos = ItemPlacerSolver.FindItemPlacePosition(checkPos, 9, blocksSpawnMask, true, 10, 0.1f);
        GameObject particle = ParticleManager.SpawnParticle(ParticleType.SmokePoof, presentItem.transform.position);
        AudioManager.Play("Desync Disappear", particle.transform);
        if(targetPos.x == float.MaxValue)
        {
            Debug.LogWarning("Could not find valid position for present item. Moving anyways");
            targetPos = checkPos;
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
        foreach (GameObject go in presentItem.enableOnDrop)
        {
            go.SetActive(false);
        }
        CoroutineUtils.ExecuteAfterEndOfFrame(() =>{
        foreach (GameObject go in presentItem.enableOnDrop)
        {
            go.SetActive(true);
        }}, this);
    }

    private Vector3 GetLocalPosition()
    {
        if(PlayerInventory.GetCurrentItem() != null && PlayerInventory.GetCurrentItem().itemName == itemName)
        {
            return Player.GetInstance().transform.localPosition;
        }
        return transform.localPosition;
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

            if(ShouldMovePresentItem())
            {
                MovePresentItemToPastLocation();
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
        // SetLayer(LayerMask.NameToLayer("Item"));
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position);
        AudioManager.Play("Desync Disappear", transform);
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
        if (currentTile == null)
        {
            return;
        }
        isDesynced = currentTile.islandId == e.desyncIslandId || currentTile.islandId == e.anchoredTileIslandId;
        UpdateItemPair();
    }

    private void OnDesyncEndWorld(object sender, MagiTechGrid.OnDesyncArgs e)
    {
        isDesynced = false;
        UpdateItemPair();
        if(ShouldMovePresentItem())
        {
            MovePresentItemToPastLocation();
        }
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
                GameObject particle = ParticleManager.SpawnParticle(ParticleType.SmokePoof, presentItem.transform.position);
                AudioManager.Play("Desync Disappear", particle.transform);
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
            if(active)
            {
                AddTracker();
            }
            else
            {
                UITrackerManager.RemoveTracker(gameObject);
            }
        }
        foreach (GameObject go in enableOnDrop)
        {
            go.SetActive(active);
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
