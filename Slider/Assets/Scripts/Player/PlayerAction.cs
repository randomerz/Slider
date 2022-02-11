using UnityEngine;

public class PlayerAction : MonoBehaviour 
{
    public static System.EventHandler<System.EventArgs> OnAction;

    private Item pickedItem;
    private bool isPicking;
    [SerializeField] private Transform itemPickupLocation;
    [SerializeField] private GameObject itemDropIndicator;
    private bool canDrop;
    
    private int actionsAvailable = 0;
    [SerializeField] private GameObject actionAvailableIndicator;

    [SerializeField] private LayerMask itemMask;
    [SerializeField] private LayerMask dropCollidingMask;
    
    private InputSettings controls;

    private void Awake() 
    {
        controls = new InputSettings();
        controls.Player.Action.performed += context => Action();
        controls.Player.CycleEquip.performed += context => CycleEquip();
    }

    private void OnEnable() 
    {
        controls.Enable();
    }

    private void OnDisable() 
    {
        controls.Disable();
    }

    private void Update() 
    {
        pickedItem = PlayerInventory.GetCurrentItem();
        if (pickedItem != null && !isPicking) 
        {

            pickedItem.gameObject.transform.position = itemPickupLocation.position;
            itemDropIndicator.transform.position = GetIndicatorLocation();
            
            // check raycast
            Vector3 raycastDir = GetIndicatorLocation() - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, raycastDir, 1.5f, dropCollidingMask);
            if (hit) 
            {
                canDrop = false;
                itemDropIndicator.SetActive(false);
            }
            else 
            {
                canDrop = true;
                itemDropIndicator.SetActive(true);
            }
        }
        else if (pickedItem == null)
        {
            itemDropIndicator.SetActive(false);
        }
    }

    private void Action() 
    {
        TryPick();
        OnAction?.Invoke(this, new System.EventArgs());
    }

    private void CycleEquip()
    {
        if (isPicking) 
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

    public void TryPick() 
    {
        if (isPicking) 
        {
            return;
        }

        Collider2D[] nodes = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), 1f, itemMask);
        if (pickedItem == null) 
        {
            // find nearest
            if (nodes.Length > 0)
            {
                isPicking = true;

                Collider2D nearest = nodes[0];
                float nearestDist = Vector3.Distance(nearest.transform.position, transform.position);
                for (int i = 1; i < nodes.Length; i++) {
                    if (Vector3.Distance(nodes[i].transform.position, transform.position) < nearestDist) {
                        nearest = nodes[i];
                        nearestDist = Vector3.Distance(nodes[i].transform.position, transform.position);
                    }
                }
                
                pickedItem = nearest.GetComponent<Item>();
                if (pickedItem == null) {
                    Debug.LogError("Picked something that isn't an Item!");
                }

                PlayerInventory.AddItem(pickedItem);
                pickedItem.PickUpItem(itemPickupLocation.transform, callback:FinishPicking);

            } 
        }
        else // pickedItem != null
        {
            // check if can drop
            if (canDrop) 
            {
                PlayerInventory.RemoveItem();
                pickedItem.DropItem(GetIndicatorLocation(), callback:pickedItem.dropCallback);
                pickedItem = null;
                itemDropIndicator.SetActive(false);
            }
        }
    }

    private void FinishPicking() 
    {
        isPicking = false;
        itemDropIndicator.SetActive(true);
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