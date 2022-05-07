using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class PlayerAction : MonoBehaviour 
{
    private static PlayerAction _instance;
    public static System.EventHandler<System.EventArgs> OnAction;

    public Item pickedItem;
    private bool isPicking;
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
        LoadBindings();

        LayerMask StileLayerMask = LayerMask.GetMask("Slider");
        LayerFilter.SetLayerMask(StileLayerMask);
    }

    public static void LoadBindings()
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            _instance.controls.LoadBindingOverridesFromJson(rebinds);
        }
        _instance.controls.Player.Action.performed += context => _instance.Action();
        _instance.controls.Player.CycleEquip.performed += context => _instance.CycleEquip();
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

    private void Update()
    {
        pickedItem = PlayerInventory.GetCurrentItem();
        if (pickedItem != null && !isPicking) 
        {

            pickedItem.gameObject.transform.position = itemPickupLocation.position;
            itemDropIndicator.transform.position = GetIndicatorLocation();

            // check raycast hitting items, npcs, houses, etc.
            Vector3 raycastDir = GetIndicatorLocation() - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, raycastDir, 1.5f, dropCollidingMask);
            if (hit) {
                canDrop = false;
                itemDropIndicator.SetActive(false);
            }
            else 
            {
                // check raycast hitting tiles that aren't active
                canDrop = true;
                itemDropIndicator.SetActive(true);
                
                int NumOfRayCastResult = Physics2D.Raycast(transform.position, raycastDir, LayerFilter, RayCastResults, 1.5f);
                if (NumOfRayCastResult != 0) 
                {
                    for (int i = 0; i < NumOfRayCastResult; i++)
                    {
                        STile stile = RayCastResults[i].collider.gameObject.GetComponent<STile>();
                        if (stile != null)
                        {
                            if (!stile.isTileActive) 
                            {
                                canDrop = false;
                                itemDropIndicator.SetActive(false);
                                break;
                            }
                        }
                    }
                }
            }
        }
        else if (pickedItem == null)
        {
            itemDropIndicator.SetActive(false);
        }
    }

    private void Action() 
    {
        if (UIManager.IsUIOpen())
            return;

        if (TryPick())
            return; // if succesfully picked something up, return

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

    public bool TryPick() 
    {
        if (isPicking) 
        {
            return false;
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

                return true;
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