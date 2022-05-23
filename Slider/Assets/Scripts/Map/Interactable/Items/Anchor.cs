using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor : Item
{
    // Start is called before the first frame update
    [SerializeField] private float shakeAmount;
    [SerializeField] private float shakeDuration;
    public Sprite trackerSprite;

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

        Player.SetMoveSpeedMultiplier(0.75f);
        PlayerInventory.Instance.SetHasCollectedAnchor(true);
        
        UITrackerManager.RemoveTracker(this.gameObject);

    }

    public void UnanchorTile()
    {
        STile hitTile = SGrid.current.GetStileUnderneath(this.gameObject);
        if (hitTile != null)
        {
            hitTile.hasAnchor = false;
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
        }

        Player.SetMoveSpeedMultiplier(1f);
        UITrackerManager.AddNewTracker(this.gameObject, trackerSprite);
        return null;
    }
    public override void dropCallback()
    {
        CameraShake.Shake(shakeDuration, shakeAmount);
        AudioManager.Play("Slide Explosion");
    }

    


}
