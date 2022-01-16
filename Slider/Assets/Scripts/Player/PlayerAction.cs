using UnityEngine;

public class PlayerAction : MonoBehaviour 
{
    private Item pickedItem;
    private bool isPicking;
    [SerializeField] private Transform itemLocation;

    [SerializeField] private LayerMask itemMask;
    
    private InputSettings controls;

    private void Awake() 
    {
        controls = new InputSettings();
        controls.Player.Action.performed += context => Action();
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
        if (pickedItem != null && !isPicking) 
        {
            pickedItem.gameObject.transform.position = itemLocation.position;
        }
    }

    private void Action() 
    {
        TryPick();
    }

    public void TryPick() 
    {
        Debug.Log("im tryna pick");
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

                pickedItem.PickUpItem(itemLocation.transform, callback:FinishPicking);
            } 
        }
        else // pickedItem != null
        {
            // check if can drop
            pickedItem.DropItem(transform.position);
            pickedItem = null;
        }
    }

    private void FinishPicking() 
    {
        isPicking = false;
    }
}