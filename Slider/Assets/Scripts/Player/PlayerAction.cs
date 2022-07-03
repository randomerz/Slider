using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class PlayerAction : MonoBehaviour 
{
    private static PlayerAction _instance;
    public static System.EventHandler<System.EventArgs> OnAction;

    public Item pickedItem;
    private Item lastDroppedItem;
    private bool isPicking; //Picking up animation is happening
    private bool isDropping;    //Drop animation is happening.
    private bool canDrop;
    [SerializeField] private Transform itemPickupLocation;
    [SerializeField] private GameObject itemDropIndicator;
    [SerializeField] private LayerMask itemMask;
    [SerializeField] private LayerMask dropCollidingMask;
    private List<RaycastHit2D> RayCastResults = new List<RaycastHit2D>();
    private ContactFilter2D LayerFilter;

    private int actionsAvailable = 0;
    [SerializeField] private GameObject actionAvailableIndicator;
    private InputSettings controls;
    // private GameObject[] objects;

    private void Awake() 
    {
        _instance = this;
        _instance.controls = new InputSettings();

        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Action, context => _instance.Action());
        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.CycleEquip, context => _instance.CycleEquip());
    }

    private void OnEnable() 
    {
        controls.Enable();
    }

    private void OnDisable() 
    {
        controls.Disable();
    }

    private void OnDestroy() 
    {
        
    }

    //L: :(
    private void Update()
    {
        pickedItem = PlayerInventory.GetCurrentItem();
        if (pickedItem != null && !isPicking) 
        {

            pickedItem.gameObject.transform.position = itemPickupLocation.position;

            Vector2 furthestValidDropPosition = GetFurthestValidDropPosition();

            canDrop = Vector2.Distance(transform.position, furthestValidDropPosition) > 0.5f;
            itemDropIndicator.SetActive(canDrop);
            itemDropIndicator.transform.position = furthestValidDropPosition;
        }
        else if (pickedItem == null)
        {
            itemDropIndicator.SetActive(false);
        }
    }

    private Vector2 GetFurthestValidDropPosition()
    {
        Vector3 raycastDirection = GetIndicatorLocation() - transform.position;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 0.5f, raycastDirection, 1.5f, layerMask:dropCollidingMask);

        Vector2 furthestPossibleDropPosition = transform.position;
        bool hitCollider = false;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject != pickedItem.gameObject && !hit.collider.isTrigger)
            {
                hitCollider = true;

                // Hit an obstacle
                if (Vector2.Distance(hit.centroid, transform.position) > Vector2.Distance(furthestPossibleDropPosition, transform.position))
                {
                    furthestPossibleDropPosition = hit.centroid;
                }
            }

            STile stile = hit.collider.gameObject.GetComponent<STile>();
            if (stile != null && !stile.isTileActive)
            {
                hitCollider = false;

                // Hit an inactive stile
                if (Vector2.Distance(hit.centroid, transform.position) > Vector2.Distance(furthestPossibleDropPosition, transform.position))
                {
                    furthestPossibleDropPosition = hit.centroid;
                }
            }
        }

        if (furthestPossibleDropPosition == (Vector2) transform.position)
        {
            if (hitCollider)
            {
                // There was a collision, but it wasn't a valid drop location
                furthestPossibleDropPosition = transform.position;
            } 
            else
            {
                // Use furthest location if there is no collision
                Vector3 moveDir = Player.GetLastMoveDir().normalized;
                if (moveDir.x != 0 && moveDir.y != 0)
                {
                    moveDir = moveDir * 0.75f * Mathf.Sqrt(2);
                }
                furthestPossibleDropPosition = transform.position + moveDir;
                Debug.Log("MAX DISTANCE");
            }
        }
        return furthestPossibleDropPosition;
    }

    private bool DoesDropRaycastIntersect()
    {
        // check raycast hitting items, npcs, houses, etc.
        Vector3 raycastDir = GetIndicatorLocation() - transform.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, raycastDir, 1.5f, dropCollidingMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject != pickedItem.gameObject && !hit.collider.isTrigger)
            {
                // hit something
                return true;
            }

            STile stile = hit.collider.gameObject.GetComponent<STile>();
            if (stile != null && !stile.isTileActive)
            {
                // hit an inactive stile
                return true;
            }
        }

        // hit nothing
        return false;
    }

    private void Action() 
    {
        if (UIManager.IsUIOpen() || UIArtifactMenus.IsArtifactOpen())
            return;

        if (TryPick())
            return; // if succesfully picked something up, return

        OnAction?.Invoke(this, new System.EventArgs());
    }

    private void CycleEquip()
    {
        if (isPicking || UIManager.IsUIOpen() || UIArtifactMenus.IsArtifactOpen()) 
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

    public bool TryPick() 
    {
        if (isPicking || isDropping) 
        {
            return false;
        }

        Collider2D[] nodes = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), 1f, itemMask);
        if (pickedItem == null) 
        {
            //Try picking an item up

            if (nodes.Length > 0)
            {
                isPicking = true;

                // find nearest item
                Collider2D nearest = nodes[0];
                float nearestDist = Vector3.Distance(nearest.transform.position, transform.position);
                for (int i = 1; i < nodes.Length; i++) {
                    if (Vector3.Distance(nodes[i].transform.position, transform.position) < nearestDist) {
                        nearest = nodes[i];
                        nearestDist = Vector3.Distance(nodes[i].transform.position, transform.position);
                    }
                }
                
                pickedItem = nearest.GetComponent<Item>();  //Not GetComponentInParent?
                if (pickedItem == null) {
                    Debug.LogError("Picked something that isn't an Item!");
                }

                PlayerInventory.AddItem(pickedItem);
                pickedItem.PickUpItem(itemPickupLocation.transform, callback:FinishPicking);

                return true;
            } 
        }
        else // pickedItem != null
        {
            // Try dropping the item
            if (canDrop) 
            {
                isDropping = true;
                PlayerInventory.RemoveItem();
                pickedItem.DropItem(itemDropIndicator.transform.position, callback:FinishDropping);
                lastDroppedItem = pickedItem;
                pickedItem = null;
                itemDropIndicator.SetActive(false);

                return true;
            }
        }

        return false;
    }

    private void FinishPicking() 
    {
        isPicking = false;
        itemDropIndicator.SetActive(true);
    }

    private void FinishDropping()
    {
        lastDroppedItem.dropCallback();
        isDropping = false;
        itemDropIndicator.SetActive(false);
    }

    private Vector3 GetIndicatorLocation() 
    {
        Vector3 moveDir = Player.GetLastMoveDir().normalized;
        if (moveDir.x != 0 && moveDir.y != 0) 
        {
            moveDir = moveDir * 0.75f * Mathf.Sqrt(2);
        }
        return transform.position + moveDir;
    }

    public bool HasItem() {
        return pickedItem != null;
    }


    public void IncrementActionsAvailable()
    {
        actionsAvailable += 1;
        if (actionsAvailable > 0)
        {
            actionAvailableIndicator.SetActive(true);
        }
    }

    public void DecrementActionsAvailable()
    {
        actionsAvailable -= 1;
        if (actionsAvailable <= 0)
        {
            actionAvailableIndicator.SetActive(false);
        }
    }


}