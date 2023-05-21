using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorFirstAppearance : MonoBehaviour
{
    public Anchor anchor;
    public SpriteRenderer spriteRenderer;
    public PlayerActionHints hints;
    public static event System.EventHandler<System.EventArgs> OnAnchorAcquire;
    void Start()
    {
        if (PlayerInventory.Instance.GetHasCollectedAnchor())
        {
            anchor.UnanchorTile();
            Destroy(gameObject);
            return;
        }
        // add to tracker
        UITrackerManager.AddNewTracker(gameObject, anchor.trackerSprite);
        
        anchor.OnPickUp.AddListener(DoCutscene);
    }

    public void DoCutscene()
    {
        ItemPickupEffect.StartCutscene(spriteRenderer.sprite, "Anchor");
        anchor.OnPickUp.RemoveListener(DoCutscene);
        OnAnchorAcquire?.Invoke(this, new System.EventArgs{});
        hints.TriggerHint("anchor");

        AchievementManager.SetAchievementStat("collectedAnchor", 1);

        Destroy(this);
    }
}
