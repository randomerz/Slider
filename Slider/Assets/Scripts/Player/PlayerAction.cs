using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerAction : Singleton<PlayerAction>
{
    public static PlayerAction Instance => _instance;

    public Item pickedItem;

    [SerializeField] private Transform pickedItemLocation;
    [SerializeField] private Transform pickedItemReflectionLocation;
    [SerializeField] private Transform boatDropItemBaseTransform;
    [SerializeField] private GameObject itemDropIndicator;
    [SerializeField] private LayerMask itemMask;
    [SerializeField] private LayerMask dropCollidingMask;
    [SerializeField] private LayerMask anchorDropCollidingMask;
    [SerializeField] private LayerMask noDropItemsLayerMask; // we want to pick up, but not drop, ex magitech portals

    [SerializeField] private float minimumDropDistance = 0.5f;
    [SerializeField] private float maximumDropDistance = 1.5f;
    [SerializeField] private float dropBoxcastWidth = 0.75f; // 12/16
    [SerializeField] private float dropBoxcastHeight = 7f / 16f;

    private Item lastDroppedItem;
    private bool isPicking;  //Picking up animation is happening
    private bool isDropping; //Drop animation is happening
    private bool canDrop;

    [SerializeField] private GameObject actionAvailableIndicator;

    private HashSet<IInteractable> availableInteractables = new();

    private int itemSortingOrder;
    private int itemSortingOrderCalls;

    private void Awake() 
    {
        InitializeSingleton(overrideExistingInstanceWith: this);

        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Action, context => _instance.Action());
        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.CycleEquip, context => _instance.CycleEquip());
    }

    private void Update()
    {
        pickedItem = PlayerInventory.GetCurrentItem();
        if (pickedItem != null && !isPicking)
        {
            Vector2 closestValidDropPosition = GetClosestValidDropPosition();
            
            // we offset where the raycast starts because when you're in the boat, the collider is at the boat not the player
            Vector3 basePosition = GetPlayerTransformRaycastPosition();

            canDrop = Vector2.Distance(basePosition, closestValidDropPosition) > minimumDropDistance;
            itemDropIndicator.SetActive(canDrop);
            itemDropIndicator.transform.position = closestValidDropPosition;
        }
        else if (pickedItem == null)
        {
            itemDropIndicator.SetActive(false);
        }
    }

    private Vector2 GetClosestValidDropPosition()
    {
        Vector3 raycastDirection = Player.GetLastMoveDirection().normalized;

        // we offset where the raycast starts because when you're in the boat, the collider is at the boat not the player
        Vector3 basePosition = GetPlayerTransformRaycastPosition();

        LayerMask lm = pickedItem is Anchor ? anchorDropCollidingMask : dropCollidingMask;
        lm |= noDropItemsLayerMask;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            basePosition, 
            new Vector2(dropBoxcastWidth, dropBoxcastHeight), 
            Mathf.Atan2(raycastDirection.y, raycastDirection.x) * Mathf.Rad2Deg,
            raycastDirection, 
            maximumDropDistance,
            layerMask: lm
        );

        Vector2 closestPossibleDropPosition = basePosition + maximumDropDistance * raycastDirection;
        bool collisionOccured = false;

        foreach (RaycastHit2D hit in hits)
        {
            Collider2D hitCollider = hit.collider;
            STile stile = hitCollider.gameObject.GetComponent<STile>();

            // Hit an obstacle or an inactive STile
            if ((hitCollider.gameObject != pickedItem.gameObject && !hitCollider.isTrigger) || (stile != null && !stile.isTileActive))
            {
                collisionOccured = true;
                if (Vector2.Distance(hit.centroid, basePosition) < Vector2.Distance(closestPossibleDropPosition, basePosition))
                {
                    closestPossibleDropPosition = hit.centroid;
                }
            }
        }

        // Use maximum distance if there is no collision
        if (!collisionOccured)
        {
            Vector3 moveDir = Player.GetLastMoveDirection().normalized;
            closestPossibleDropPosition = basePosition + moveDir * maximumDropDistance;
        }

        return closestPossibleDropPosition;
    }

    public Vector3 GetPlayerTransformRaycastPosition()
    {
        if (!Player.GetInstance().GetIsOnWater())
        {
            return transform.position;
        }
        else
        {
            return boatDropItemBaseTransform.transform.position;
        }
    }

    private void Action() 
    {
        if (PauseManager.IsPaused || UIArtifactMenus.IsArtifactOpen() || !Player.GetCanMove())
            return;

        bool successfullyPickedUpItem = AttemptItemPickup();
        bool successfullyInteractedWithAnInteractable = Interact();

        if (!successfullyInteractedWithAnInteractable && !successfullyPickedUpItem)
        {
            AttemptItemDrop();
        }
    }

    private void CycleEquip()
    {
        if (isPicking || PauseManager.IsPaused || UIArtifactMenus.IsArtifactOpen()) 
        {
            return;
        }
        
        if (pickedItem == null || pickedItem.canKeep)
        {
            PlayerInventory.NextItem();
        }
        else
        {
            AudioManager.Play("Artifact Error");
        }
    }

    private bool AttemptItemPickup()
    {
        if (isPicking || isDropping || pickedItem != null)
        {
            return false;
        }

        List<Collider2D> collidersInRange = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), 1f, itemMask).ToList();
        if (collidersInRange.Count > 0)
        {
            isPicking = true;

            pickedItem = GetNearestValidItem(collidersInRange);

            if (pickedItem == null)
            {
                isPicking = false;
                return false;
            }

            PlayerInventory.AddItem(pickedItem);
            pickedItem.SetSortingOrder(itemSortingOrder);
            pickedItem.PickUpItem(pickedItemLocation.transform, callback: FinishPicking);

            AudioManager.PlayWithPitch("UI Click", 1.2f);

            return true;
        }

        return false;
    }

    private Item GetNearestValidItem(List<Collider2D> collidersInRange)
    {
        collidersInRange.OrderBy((item) => Vector3.Distance(item.transform.position, transform.position))
                                         .Select((collider) => collider.GetComponent<Item>());
        foreach(Collider2D c in collidersInRange) {
            if(CheckItemRaycast(c))
                return c.GetComponent<Item>();
        }
        
        return null;
    }

    private bool CheckItemRaycast(Collider2D item)
    {
        if(item == null || item.GetComponent<Item>() == null) return false;        
        Vector3 itemPos = item.transform.position;
        Vector3 direction = itemPos - transform.position;
        Vector3 perp = Vector3.Cross(direction, Vector3.forward).normalized * item.GetComponent<Item>().itemRadius;

        LayerMask overrideMask = item.GetComponent<Item>().pickupOverrideLayerMask;
        LayerMask layerMask = item.GetComponent<Anchor>() ? anchorDropCollidingMask : dropCollidingMask;
        layerMask = overrideMask != 0 ? overrideMask : layerMask;

        RaycastHit2D[] h1 = Physics2D.RaycastAll(transform.position, direction, direction.magnitude, layerMask);
        RaycastHit2D[] h2 = Physics2D.RaycastAll(transform.position + perp, direction, direction.magnitude, layerMask);
        RaycastHit2D[] h3 = Physics2D.RaycastAll(transform.position - perp, direction, direction.magnitude, layerMask);

        return (CheckRaycastList(h1, item) || CheckRaycastList(h2, item) || CheckRaycastList(h3, item));
    }

    private bool CheckRaycastList(RaycastHit2D[] hits, Collider2D item)
    {
        foreach (RaycastHit2D hit in hits)
        {
            Collider2D hitCollider = hit.collider;
            if (!hitCollider.isTrigger && hitCollider != item) {
                return false;
            }
        }

        return true;
    }

    private bool AttemptItemDrop()
    {
        if (canDrop && pickedItem != null && !isPicking)
        {
            isDropping = true;
            PlayerInventory.RemoveItem();
            pickedItem.DropItem(itemDropIndicator.transform.position, callback: FinishDropping);
            lastDroppedItem = pickedItem;
            itemDropIndicator.SetActive(false);
            pickedItem.SetLayer(LayerMask.NameToLayer("Item"));
            AudioManager.PlayWithPitch("UI Click", 0.8f);

            return true;
        }

        return false;
    }

    private void FinishPicking()
    {
        isPicking = false;
        // If item is destroyed while being picked up
        if (pickedItem != null && !pickedItem.isQueuedForDestruction)
        {
            pickedItem.transform.SetParent(pickedItemLocation);
            pickedItem.SetLayer(LayerMask.NameToLayer("ItemRT"));
        }
    }

    private void FinishDropping()
    {
        lastDroppedItem.ResetSortingOrder();
        lastDroppedItem.dropCallback();
        isDropping = false;
        if (itemDropIndicator != null) // gets destroyed during scene transitions lol
        {
            itemDropIndicator.SetActive(false);
        }
        pickedItem = null;
    }

    public void SetItemSortingOrderInc(int num)
    {
        itemSortingOrderCalls++;
        SetItemSortingOrder(num);
    }

    public void SetItemSortingOrderDec(int num)
    {
        itemSortingOrderCalls--;
        if(itemSortingOrderCalls == 0)
            SetItemSortingOrder(num);

    }

    public void SetItemSortingOrder(int num)
    {
        itemSortingOrder = num;
        PlayerInventory.Instance.SetItemSortingOrder(num);
    }

    public Transform GetPickedItemLocationTransform()
    {
        return pickedItemLocation;
    }

    public Transform GetPickedItemReflectionLocationTransform()
    {
        return pickedItemReflectionLocation;
    }


    public bool HasItem()
    {
        return pickedItem != null;
    }

    public bool HasItem(string itemName)
    {
        return pickedItem != null && pickedItem.itemName.Equals(itemName);
    }

    /// <summary>
    /// Duplicate interactables are not stored; adding an interactable multiple times will have no effect.
    /// </summary>
    public void AddInteractable(IInteractable interactable)
    {
        availableInteractables.Add(interactable);
        UpdateActionsAvailableIndicator();
    }

    public void RemoveInteractable(IInteractable interactable)
    {
        availableInteractables.Remove(interactable);
        UpdateActionsAvailableIndicator();
    }

    public void UpdateActionsAvailableIndicator()
    {
        actionAvailableIndicator.SetActive(availableInteractables.ToList().FindAll((interactable) => interactable.DisplayInteractionPrompt).Count > 0);
    }

    private bool Interact()
    {
        bool successfullyInteracted = false;

        if (availableInteractables.Count > 0)
        {
            List<IInteractable> interactables = availableInteractables.ToList();

            int highestPriority = interactables.Select((interactable) => interactable.InteractionPriority).Max();

            List<IInteractable> interactablesWithSharedHighestPriority
                = interactables.FindAll((interactable) => interactable.InteractionPriority == highestPriority);

            interactablesWithSharedHighestPriority.ForEach((interactable) =>
            {
                if (interactable.Interact())
                {
                    successfullyInteracted = true;
                }
            });
        }

        return successfullyInteracted;
    }
}