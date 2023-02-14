using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor : Item
{
    public class OnAnchorInteractArgs : System.EventArgs
    {
        public STile stile;
        public bool drop;
    }

    public static event System.EventHandler<OnAnchorInteractArgs> OnAnchorInteract;
    
    
    // Start is called before the first frame update
    [SerializeField] private float shakeAmount;
    [SerializeField] private float shakeDuration;
    //[SerializeField] private ConductiveElectricalNode conductiveNode;
    public Sprite trackerSprite;
    private STile currentSTile; //C: used so it can be passed as a parameter in OnAnchorDrop

    public void Start()
    {
        if (!SaveSystem.Current.GetBool("playerHasCollectedAnchor"))
        {
            currentSTile = GetComponentInParent<STile>();
            if (currentSTile != null)
            {
                currentSTile.hasAnchor = true;
                if(currentSTile.isTileActive)
                    OnAnchorInteract?.Invoke(this, new OnAnchorInteractArgs { stile = currentSTile, drop=true });
            }
        }
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        if (Player.GetInstance() != null)
        {
            Player.SetMoveSpeedMultiplier(1f);
        }
    }

    public override void PickUpItem(Transform pickLocation, System.Action callback = null) // pickLocation may be moving
    {
        base.PickUpItem(pickLocation, callback);
        OnAnchorInteract?.Invoke(this, new OnAnchorInteractArgs { stile = currentSTile, drop=false });
        UnanchorTile();
        

        Player.SetMoveSpeedMultiplier(0.75f);
        PlayerInventory.Instance.SetHasCollectedAnchor(true);
        
        UITrackerManager.RemoveTracker(this.gameObject);
    }

    public void UnanchorTile()
    {
        if(currentSTile != null)
            currentSTile.hasAnchor = false;
        currentSTile = null;
    }


    public override void OnEquip()
    {
        base.OnEquip();
        Player.SetMoveSpeedMultiplier(0.75f);
    }

    public override STile DropItem(Vector3 dropLocation, System.Action callback = null)
    {
        STile hitTile = base.DropItem(dropLocation, callback);
        if (hitTile != null)
        {
            hitTile.hasAnchor = true;
            currentSTile = hitTile;
        }

        Player.SetMoveSpeedMultiplier(1f);
        UITrackerManager.AddNewTracker(this.gameObject, trackerSprite);
        return null;
    }
    
    public override void dropCallback()
    {
        base.dropCallback();
        CameraShake.Shake(shakeDuration, shakeAmount);
        AudioManager.Play("Slide Explosion");
        
        OnAnchorInteract?.Invoke(this, new OnAnchorInteractArgs { stile = currentSTile, drop=true });
    }
}
