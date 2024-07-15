using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor : Item
{
    public class OnAnchorInteractArgs : System.EventArgs
    {
        public STile stile;
        public bool drop;
        public bool fromStart = false;
    }

    public static event System.EventHandler<OnAnchorInteractArgs> OnAnchorInteract;
    
    [SerializeField] private float shakeAmount;
    [SerializeField] private float shakeDuration;
    public Sprite trackerSprite;
    private STile currentSTile; 

    public ParticleSystem splashParticle;
    private bool shouldSpawnSplashParticle = false;

    public override void Start()
    {
        base.Start();

        if (!SaveSystem.Current.GetBool("playerHasCollectedAnchor"))
        {
            currentSTile = GetComponentInParent<STile>();
            if (currentSTile != null)
            {
                currentSTile.hasAnchor = true;
                if(currentSTile.isTileActive)
                    OnAnchorInteract?.Invoke(this, new OnAnchorInteractArgs { stile = currentSTile, drop=true, fromStart = true });
            }
        }
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

        RemoveFromTile();
        Player.SetMoveSpeedMultiplier(0.75f);
        PlayerInventory.Instance.SetHasCollectedAnchor(true);
    }

    public void RemoveFromTile()
    {
        OnAnchorInteract?.Invoke(this, new OnAnchorInteractArgs { stile = currentSTile, drop=false });
        UnanchorTile();
        UITrackerManager.RemoveTracker(gameObject);
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
        AddToTile(hitTile);

        Player.SetMoveSpeedMultiplier(1f);
        if(Player.GetInstance().GetIsOnWater())
        {
            shouldSpawnSplashParticle = true;
        }
        return hitTile;
    }

    public void AddToTile(STile hitTile)
    {
        if (hitTile != null)
        {
            hitTile.hasAnchor = true;
            currentSTile = hitTile;
        }

        UITrackerManager.AddNewTracker(gameObject, trackerSprite);
    }

    public override void dropCallback()
    {
        base.dropCallback();
        CameraShake.Shake(shakeDuration, shakeAmount);
        AudioManager.Play("Slide Explosion");
        if(shouldSpawnSplashParticle)
        {
            SpawnSplashParticle();
        }
        
        OnAnchorInteract?.Invoke(this, new OnAnchorInteractArgs { stile = currentSTile, drop=true });
    }

    private void SpawnSplashParticle()
    {
        splashParticle.Play();
        AudioManager.Play("Boat Splash");
        shouldSpawnSplashParticle = false;
    }

    public void DropThroughIce()
    {
        RemoveFromTile();
        AddToTile(SGrid.GetSTileUnderneath(this.transform, null));
        OnAnchorInteract?.Invoke(this, new OnAnchorInteractArgs { stile = currentSTile, drop=true });
    }
}
