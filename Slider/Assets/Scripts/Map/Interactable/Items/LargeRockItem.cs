using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeRockItem : Item
{
    // Start is called before the first frame update
    [SerializeField] private float shakeAmount;
    [SerializeField] private float shakeDuration;

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
        
        Player.SetMoveSpeedMultiplier(0.75f);
    }

    public override STile DropItem(Vector3 dropLocation, System.Action callback = null)
    {
        STile hitTile = base.DropItem(dropLocation, callback);

        Player.SetMoveSpeedMultiplier(1f);
        return null;
    }
    public override void dropCallback()
    {
        base.dropCallback();
        CameraShake.Shake(shakeDuration, shakeAmount);
        AudioManager.Play("Slide Explosion");
    }
}
