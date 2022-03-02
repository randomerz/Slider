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
        GetComponentInParent<STile>().hasAnchor = true;
        UITrackerManager.addNewTracker(this.gameObject, trackerSprite);
    }

    private void OnDisable()
    {
        Player.setMoveSpeedMultiplier(1f);
    }

    public override void PickUpItem(Transform pickLocation, System.Action callback = null) // pickLocation may be moving
    {
        Debug.Log("Anchor");
        STile[,] tiles = SGrid.current.GetGrid();
        foreach (STile tile in tiles)
        {
            tile.hasAnchor = false;
        }
        base.PickUpItem(pickLocation, callback);

        Player.setMoveSpeedMultiplier(0.75f);

    }

    public override void OnEquip()
    {
        Player.setMoveSpeedMultiplier(0.75f);
    }

    public override STile DropItem(Vector3 dropLocation, System.Action callback = null)
    {
        STile hitTile = base.DropItem(dropLocation, callback);
        if (hitTile != null)
        {
            hitTile.hasAnchor = true;
        }

        Player.setMoveSpeedMultiplier(1f);

        return null;
    }

    public override void dropCallback()
    {
        CameraShake.Shake(shakeDuration, shakeAmount);
        AudioManager.Play("Slide Explosion");
    }

    


}
