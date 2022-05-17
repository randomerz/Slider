using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor : Item
{
    public class OnAnchorDropArgs : System.EventArgs
    {
        public STile stile;
    }
    public static event System.EventHandler<OnAnchorDropArgs> OnAnchorDrop;

    // Start is called before the first frame update
    [SerializeField] private float shakeAmount;
    [SerializeField] private float shakeDuration;
    public Sprite trackerSprite;
    private STile currentSTile; //C: used so it can be passed as a parameter in OnAnchorDrop

    public void Start()
    {
        if (GetComponentInParent<STile>() != null)
            GetComponentInParent<STile>().hasAnchor = true;
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
        UnanchorTile();
        currentSTile = null;

        Player.SetMoveSpeedMultiplier(0.75f);
        PlayerInventory.Instance.SetHasCollectedAnchor(true);
        
        UITrackerManager.RemoveTracker(this.gameObject);

    }

    public void UnanchorTile()
    {
        STile[,] tiles = SGrid.current.GetGrid();
        foreach (STile tile in tiles)
        {
            tile.hasAnchor = false;
        }
    }


    public override void OnEquip()
    {
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
        CameraShake.Shake(shakeDuration, shakeAmount);
        AudioManager.Play("Slide Explosion");
        OnAnchorDrop?.Invoke(this, new OnAnchorDropArgs { stile = currentSTile });
    }

    


}
